using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class XuatKhoDAL
    {
        private readonly TonKhoDAL tonKhoDAL = new TonKhoDAL();

        public int ThemPhieuXuat(PhieuXuatKho phieu, List<ChiTietPhieuXuatKho> chiTietList)
        {
            if (phieu == null) throw new ArgumentNullException(nameof(phieu));
            if (chiTietList == null || chiTietList.Count == 0) throw new ArgumentException("Danh sách chi tiết xuất kho không được trống");

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    int maPX;
                    const string insertPhieu = @"
                        INSERT INTO PhieuXuatKho (NgayXuat, MaTK, LyDo, GhiChu)
                        VALUES (@NgayXuat, @MaTK, @LyDo, @GhiChu);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(insertPhieu, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@NgayXuat", phieu.NgayXuat);
                        cmd.Parameters.AddWithValue("@MaTK", (object)phieu.MaTK ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LyDo", (object)phieu.LyDo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GhiChu", (object)phieu.GhiChu ?? DBNull.Value);

                        object result = cmd.ExecuteScalar();
                        maPX = Convert.ToInt32(result);
                    }

                    const string insertChiTiet = @"
                        INSERT INTO ChiTietPhieuXuatKho (MaPX, MaMon, SoLuong, DonGia)
                        VALUES (@MaPX, @MaMon, @SoLuong, @DonGia);";

                    foreach (var ct in chiTietList)
                    {
                        tonKhoDAL.TruTon(ct.MaMon, ct.SoLuong, conn, transaction);

                        using (SqlCommand cmd = new SqlCommand(insertChiTiet, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MaPX", maPX);
                            cmd.Parameters.AddWithValue("@MaMon", ct.MaMon);
                            cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                            cmd.Parameters.AddWithValue("@DonGia", (object)ct.DonGia ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return maPX;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public DataTable LayDanhSachPhieuXuat(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var dt = new DataTable();
            string query = @"
                SELECT px.MaPX, px.NgayXuat, px.LyDo, px.GhiChu, tk.TenTK
                FROM PhieuXuatKho px
                LEFT JOIN TaiKhoan tk ON px.MaTK = tk.MaTK
                WHERE 1=1";

            if (tuNgay.HasValue) query += " AND CAST(px.NgayXuat AS DATE) >= @TuNgay";
            if (denNgay.HasValue) query += " AND CAST(px.NgayXuat AS DATE) <= @DenNgay";
            query += " ORDER BY px.NgayXuat DESC";

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

        public DataTable LayChiTietPhieuXuat(int maPX)
        {
            var dt = new DataTable();
            const string query = @"
                SELECT ct.MaPX, ct.MaMon, ma.TenMon, ct.SoLuong, ct.DonGia,
                       CASE WHEN ct.DonGia IS NULL THEN NULL ELSE (ct.SoLuong * ct.DonGia) END AS ThanhTien
                FROM ChiTietPhieuXuatKho ct
                JOIN MonAn ma ON ma.MaMon = ct.MaMon
                WHERE ct.MaPX = @MaPX
                ORDER BY ma.TenMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@MaPX", maPX);
                conn.Open();
                da.Fill(dt);
            }

            return dt;
        }
    }
}
