using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class NhapKhoDAL
    {
        private readonly TonKhoDAL tonKhoDAL = new TonKhoDAL();

        public int ThemPhieuNhap(PhieuNhapKho phieu, List<ChiTietPhieuNhapKho> chiTietList)
        {
            if (phieu == null) throw new ArgumentNullException(nameof(phieu));
            if (chiTietList == null || chiTietList.Count == 0) throw new ArgumentException("Danh sách chi tiết nhập kho không được trống");

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    int maPN;
                    const string insertPhieu = @"
                        INSERT INTO PhieuNhapKho (NgayNhap, MaTK, GhiChu, TongTien)
                        VALUES (@NgayNhap, @MaTK, @GhiChu, @TongTien);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertPhieu, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@NgayNhap", phieu.NgayNhap);
                        cmd.Parameters.AddWithValue("@MaTK", (object)phieu.MaTK ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GhiChu", (object)phieu.GhiChu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TongTien", phieu.TongTien);

                        object result = cmd.ExecuteScalar();
                        maPN = Convert.ToInt32(result);
                    }

                    const string insertChiTiet = @"
                        INSERT INTO ChiTietPhieuNhapKho (MaPN, MaMon, SoLuong, DonGia)
                        VALUES (@MaPN, @MaMon, @SoLuong, @DonGia);";

                    foreach (var ct in chiTietList)
                    {
                        using (SqlCommand cmd = new SqlCommand(insertChiTiet, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MaPN", maPN);
                            cmd.Parameters.AddWithValue("@MaMon", ct.MaMon);
                            cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@DonGia", ct.DonGia);
                            cmd.ExecuteNonQuery();
                        }

                        tonKhoDAL.CongTon(ct.MaMon, ct.SoLuong, conn, transaction);
                    }

                    transaction.Commit();
                    return maPN;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public DataTable LayDanhSachPhieuNhap(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var dt = new DataTable();
            string query = @"
                SELECT pn.MaPN, pn.NgayNhap, pn.TongTien, pn.GhiChu, tk.TenTK
                FROM PhieuNhapKho pn
                LEFT JOIN TaiKhoan tk ON pn.MaTK = tk.MaTK
                WHERE 1=1";

            if (tuNgay.HasValue) query += " AND CAST(pn.NgayNhap AS DATE) >= @TuNgay";
            if (denNgay.HasValue) query += " AND CAST(pn.NgayNhap AS DATE) <= @DenNgay";
            query += " ORDER BY pn.NgayNhap DESC";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                if (tuNgay.HasValue) cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value.Date);
                if (denNgay.HasValue) cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value.Date);
                conn.Open();
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable LayChiTietPhieuNhap(int maPN)
        {
            var dt = new DataTable();
            const string query = @"
                SELECT ct.MaPN, ct.MaMon, ma.TenMon, ct.SoLuong, ct.DonGia,
                       (ct.SoLuong * ct.DonGia) AS ThanhTien
                FROM ChiTietPhieuNhapKho ct
                JOIN MonAn ma ON ma.MaMon = ct.MaMon
                WHERE ct.MaPN = @MaPN
                ORDER BY ma.TenMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@MaPN", maPN);
                conn.Open();
                da.Fill(dt);
            }

            return dt;
        }
    }
}
