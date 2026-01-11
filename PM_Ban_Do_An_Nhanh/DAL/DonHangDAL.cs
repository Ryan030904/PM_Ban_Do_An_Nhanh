using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class DonHangDAL
    {
        private void EnsureSoLuongTonColumn(SqlConnection conn, SqlTransaction transaction)
        {
            const string sql = @"
IF COL_LENGTH('MonAn','SoLuongTon') IS NULL
BEGIN
    ALTER TABLE MonAn ADD SoLuongTon INT NOT NULL CONSTRAINT DF_MonAn_SoLuongTon DEFAULT(0);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public int ThemDonHang(DonHang donHang, List<ChiTietDonHang> chiTietList)
        {
            int maDonHangMoi = -1;
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Ensure inventory column exists
                    try { EnsureSoLuongTonColumn(conn, transaction); } catch { }

                    // Check for potential duplicate orders within last 30 seconds
                    if (KiemTraDonHangTrungLap(donHang, conn, transaction))
                    {
                        throw new InvalidOperationException("Đơn hàng tương tự đã được tạo trong vòng 30 giây qua. Vui lòng kiểm tra lại.");
                    }

                    // Validate & decrement stock first (so we don't create orders for out-of-stock items)
                    foreach (var chiTiet in chiTietList)
                    {
                        const string queryStock = @"SELECT ISNULL(SoLuongTon,0) FROM MonAn WITH (UPDLOCK, ROWLOCK) WHERE MaMon = @MaMon";
                        using (SqlCommand cmdStock = new SqlCommand(queryStock, conn, transaction))
                        {
                            cmdStock.Parameters.AddWithValue("@MaMon", chiTiet.MaMon);
                            object stockObj = cmdStock.ExecuteScalar();
                            int stock = stockObj == null || stockObj == DBNull.Value ? 0 : Convert.ToInt32(stockObj);

                            if (stock < chiTiet.SoLuong)
                            {
                                throw new InvalidOperationException(
                                    $"Món '{chiTiet.TenMon}' không đủ tồn kho. Tồn: {stock}, cần: {chiTiet.SoLuong}.");
                            }
                        }

                        const string queryDec = @"
UPDATE MonAn
SET SoLuongTon = ISNULL(SoLuongTon,0) - @SoLuong,
    TrangThai = CASE WHEN (ISNULL(SoLuongTon,0) - @SoLuong) > 0 THEN N'Còn hàng' ELSE N'Hết hàng' END
WHERE MaMon = @MaMon";
                        using (SqlCommand cmdDec = new SqlCommand(queryDec, conn, transaction))
                        {
                            cmdDec.Parameters.AddWithValue("@MaMon", chiTiet.MaMon);
                            cmdDec.Parameters.AddWithValue("@SoLuong", chiTiet.SoLuong);
                            cmdDec.ExecuteNonQuery();
                        }
                    }

                    // Insert main order
                    string queryDonHang = @"
                        INSERT INTO DonHang (NgayLap, TongTien, TrangThaiThanhToan, MaKH) 
                        VALUES (@NgayLap, @TongTien, @TrangThaiThanhToan, @MaKH); 
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmdDonHang = new SqlCommand(queryDonHang, conn, transaction))
                    {
                        cmdDonHang.Parameters.AddWithValue("@NgayLap", donHang.NgayLap);
                        cmdDonHang.Parameters.AddWithValue("@TongTien", donHang.TongTien);
                        cmdDonHang.Parameters.AddWithValue("@TrangThaiThanhToan", donHang.TrangThaiThanhToan);
                        cmdDonHang.Parameters.AddWithValue("@MaKH", (object)donHang.MaKH ?? DBNull.Value);

                        object result = cmdDonHang.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            maDonHangMoi = Convert.ToInt32(result);
                        }
                        else
                        {
                            throw new Exception("Không thể tạo đơn hàng mới");
                        }
                    }

                    // Insert order details
                    string queryChiTiet = @"
                        INSERT INTO ChiTietDonHang (MaDH, MaMon, SoLuong, DonGia) 
                        VALUES (@MaDH, @MaMon, @SoLuong, @DonGia)";

                    foreach (var chiTiet in chiTietList)
                    {
                        using (SqlCommand cmdChiTiet = new SqlCommand(queryChiTiet, conn, transaction))
                        {
                            cmdChiTiet.Parameters.AddWithValue("@MaDH", maDonHangMoi);
                            cmdChiTiet.Parameters.AddWithValue("@MaMon", chiTiet.MaMon);
                            cmdChiTiet.Parameters.AddWithValue("@SoLuong", chiTiet.SoLuong);
                            cmdChiTiet.Parameters.AddWithValue("@DonGia", chiTiet.DonGia);
                            cmdChiTiet.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Lỗi khi thêm đơn hàng: " + ex.Message);
                }
            }
            return maDonHangMoi;
        }

        private bool KiemTraDonHangTrungLap(DonHang donHang, SqlConnection conn, SqlTransaction transaction)
        {
            string query = @"
                SELECT COUNT(*) FROM DonHang 
                WHERE TongTien = @TongTien 
                AND MaKH = @MaKH 
                AND TrangThaiThanhToan = @TrangThaiThanhToan
                AND ABS(DATEDIFF(SECOND, NgayLap, @NgayLap)) <= 30";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@TongTien", donHang.TongTien);
                cmd.Parameters.AddWithValue("@MaKH", (object)donHang.MaKH ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TrangThaiThanhToan", donHang.TrangThaiThanhToan);
                cmd.Parameters.AddWithValue("@NgayLap", donHang.NgayLap);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public int XoaDonHangTrungLap()
        {
            try
            {
                string query = @"
                    WITH DonHangDups AS (
                        SELECT MaDH, NgayLap, TongTien, MaKH,
                               ROW_NUMBER() OVER (
                                   PARTITION BY TongTien, ISNULL(MaKH, -1), 
                                                CONVERT(DATE, NgayLap), 
                                                DATEPART(HOUR, NgayLap), 
                                                DATEPART(MINUTE, NgayLap)
                                   ORDER BY MaDH
                               ) as RowNum
                        FROM DonHang
                        WHERE TrangThaiThanhToan = N'Đã thanh toán'
                    )
                    DELETE FROM ChiTietDonHang 
                    WHERE MaDH IN (
                        SELECT MaDH FROM DonHangDups WHERE RowNum > 1
                    );
                    
                    WITH DonHangDups AS (
                        SELECT MaDH, NgayLap, TongTien, MaKH,
                               ROW_NUMBER() OVER (
                                   PARTITION BY TongTien, ISNULL(MaKH, -1), 
                                                CONVERT(DATE, NgayLap), 
                                                DATEPART(HOUR, NgayLap), 
                                                DATEPART(MINUTE, NgayLap)
                                   ORDER BY MaDH
                               ) as RowNum
                        FROM DonHang
                        WHERE TrangThaiThanhToan = N'Đã thanh toán'
                    )
                    DELETE FROM DonHang 
                    WHERE MaDH IN (
                        SELECT MaDH FROM DonHangDups WHERE RowNum > 1
                    )";

                using (SqlConnection conn = DBConnection.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public DataTable LayChiTietDonHangChoIn(int maDH)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT
                    dh.MaDH,
                    dh.NgayLap,
                    dh.TongTien,
                    dh.TrangThaiThanhToan,
                    kh.TenKH,
                    kh.SDT AS SDT_KhachHang,
                    ma.TenMon,
                    ct.SoLuong,
                    ct.DonGia,
                    (ct.SoLuong * ct.DonGia) AS ThanhTien
                FROM DonHang dh
                LEFT JOIN KhachHang kh ON dh.MaKH = kh.MaKH
                JOIN ChiTietDonHang ct ON dh.MaDH = ct.MaDH
                JOIN MonAn ma ON ct.MaMon = ma.MaMon
                WHERE dh.MaDH = @MaDH
                ORDER BY ma.TenMon";

            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaDH", maDH);
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable LayDanhSachDonHang(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT DISTINCT
                    dh.MaDH,
                    dh.NgayLap,
                    dh.TongTien,
                    dh.TrangThaiThanhToan,
                    kh.TenKH,
                    kh.SDT AS SDT_KhachHang
                FROM DonHang dh
                LEFT JOIN KhachHang kh ON dh.MaKH = kh.MaKH
                WHERE 1=1";

            if (tuNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) >= @TuNgay";
            }
            if (denNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) <= @DenNgay";
            }
            query += " ORDER BY dh.NgayLap DESC";

            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (tuNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value.Date);
                    }
                    if (denNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value.Date);
                    }
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable LayThongKeDoanhThu(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT
                    CAST(dh.NgayLap AS DATE) AS Ngay,
                    SUM(dh.TongTien) AS DoanhThuNgay
                FROM DonHang dh
                WHERE dh.TrangThaiThanhToan = N'Đã thanh toán'";

            if (tuNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) >= @TuNgay";
            }
            if (denNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) <= @DenNgay";
            }
            query += " GROUP BY CAST(dh.NgayLap AS DATE) ORDER BY Ngay";

            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (tuNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value.Date);
                    }
                    if (denNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value.Date);
                    }
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public DataTable LayMonAnBanChay(DateTime? tuNgay = null, DateTime? denNgay = null, int topN = 5)
        {
            DataTable dt = new DataTable();
            string query = $@"
                SELECT TOP {topN}
                    ma.TenMon,
                    SUM(ct.SoLuong) AS TongSoLuongBan,
                    SUM(ct.SoLuong * ct.DonGia) AS TongDoanhThuMon
                FROM ChiTietDonHang ct
                JOIN MonAn ma ON ct.MaMon = ma.MaMon
                JOIN DonHang dh ON ct.MaDH = dh.MaDH
                WHERE dh.TrangThaiThanhToan = N'Đã thanh toán'";

            if (tuNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) >= @TuNgay";
            }
            if (denNgay.HasValue)
            {
                query += " AND CAST(dh.NgayLap AS DATE) <= @DenNgay";
            }
            query += " GROUP BY ma.TenMon ORDER BY TongSoLuongBan DESC";

            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (tuNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value.Date);
                    }
                    if (denNgay.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value.Date);
                    }
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }
        public int XoaTatCaDonHang()
        {
            try
            {
                string query = @"
                    DELETE FROM ChiTietDonHang;
                    DELETE FROM DonHang;";

                using (SqlConnection conn = DBConnection.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int LaySoLanMuaTheoMaKH(int maKH)
        {
            const string query = @"SELECT COUNT(*) FROM DonHang WHERE MaKH = @MaKH AND TrangThaiThanhToan = N'Đã thanh toán'";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaKH", maKH);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return 0;
                return Convert.ToInt32(result);
            }
        }
    }
}