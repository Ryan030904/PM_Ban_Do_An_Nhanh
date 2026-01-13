using System;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class MonAnDAL
    {
        private void ReseedMonAnIdentityIfNeeded(SqlConnection conn)
        {
            const string sql = @"
DECLARE @maxId INT = ISNULL((SELECT MAX(MaMon) FROM MonAn), 0);
DECLARE @currentId INT = ISNULL(CONVERT(INT, IDENT_CURRENT('MonAn')), 0);
IF (@currentId > @maxId)
BEGIN
    DECLARE @cmd NVARCHAR(200) = N'DBCC CHECKIDENT (''MonAn'', RESEED, ' + CAST(@maxId AS NVARCHAR(20)) + N')';
    EXEC(@cmd);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureSoLuongTonColumn(SqlConnection conn)
        {
            const string sql = @"
IF COL_LENGTH('MonAn','SoLuongTon') IS NULL
BEGIN
    ALTER TABLE MonAn ADD SoLuongTon INT NOT NULL CONSTRAINT DF_MonAn_SoLuongTon DEFAULT(0);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            const string seedSql = @"
IF NOT EXISTS (SELECT 1 FROM MonAn WHERE ISNULL(SoLuongTon, 0) > 0)
   AND EXISTS (SELECT 1 FROM MonAn WHERE TrangThai = N'Còn hàng')
BEGIN
    UPDATE MonAn
    SET SoLuongTon = 50
    WHERE TrangThai = N'Còn hàng'
      AND ISNULL(SoLuongTon, 0) = 0;
END";

            using (SqlCommand cmd = new SqlCommand(seedSql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable LayDanhSachMonAn()
        {
            DataTable dt = new DataTable();
            string query = "SELECT MaMon, TenMon, Gia, M.MaDM, TenDM, TrangThai, HinhAnh, ISNULL(M.SoLuongTon, 0) AS SoLuongTon " +
                           "FROM MonAn M JOIN DanhMuc DM ON M.MaDM = DM.MaDM";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                conn.Open();
                try { EnsureSoLuongTonColumn(conn); } catch { }
                da.Fill(dt);
            }

            return dt;
        }

        public int ThemMonAn(MonAn monAn)
        {
            const string query =
                "INSERT INTO MonAn (TenMon, Gia, MaDM, TrangThai, HinhAnh, SoLuongTon) " +
                "VALUES (@TenMon, @Gia, @MaDM, @TrangThai, @HinhAnh, @SoLuongTon); " +
                "SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TenMon", monAn.TenMon);
                cmd.Parameters.AddWithValue("@Gia", monAn.Gia);
                cmd.Parameters.AddWithValue("@MaDM", monAn.MaDM);
                cmd.Parameters.AddWithValue("@TrangThai", monAn.TrangThai);
                cmd.Parameters.AddWithValue("@HinhAnh",
                    string.IsNullOrEmpty(monAn.HinhAnh) ? (object)DBNull.Value : monAn.HinhAnh);
                cmd.Parameters.AddWithValue("@SoLuongTon", 0);

                conn.Open();
                try { EnsureSoLuongTonColumn(conn); } catch { }
                try { ReseedMonAnIdentityIfNeeded(conn); } catch { }
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public bool SuaMonAn(MonAn monAn)
        {
            const string query =
                "UPDATE MonAn SET TenMon = @TenMon, Gia = @Gia, MaDM = @MaDM, " +
                "TrangThai = @TrangThai, HinhAnh = @HinhAnh WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TenMon", monAn.TenMon);
                cmd.Parameters.AddWithValue("@Gia", monAn.Gia);
                cmd.Parameters.AddWithValue("@MaDM", monAn.MaDM);
                cmd.Parameters.AddWithValue("@TrangThai", monAn.TrangThai);
                cmd.Parameters.AddWithValue("@MaMon", monAn.MaMon);
                cmd.Parameters.AddWithValue("@HinhAnh",
                    string.IsNullOrEmpty(monAn.HinhAnh) ? (object)DBNull.Value : monAn.HinhAnh);

                conn.Open();
                try { EnsureSoLuongTonColumn(conn); } catch { }
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool NhapHang(int maMon, int soLuongNhap)
        {
            if (maMon <= 0) return false;
            if (soLuongNhap <= 0) return false;

            const string query = @"
UPDATE MonAn
SET SoLuongTon = ISNULL(SoLuongTon, 0) + @SoLuong,
    TrangThai = CASE WHEN (ISNULL(SoLuongTon, 0) + @SoLuong) > 0 THEN N'Còn hàng' ELSE TrangThai END
WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                cmd.Parameters.AddWithValue("@SoLuong", soLuongNhap);

                conn.Open();
                EnsureSoLuongTonColumn(conn);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool XoaMonAn(int maMon)
        {
            const string query = "DELETE FROM MonAn WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    try { ReseedMonAnIdentityIfNeeded(conn); } catch { }
                }
                return rowsAffected > 0;
            }
        }

        public string LayDuongDanAnhByMaMon(int maMon)
        {
            const string query = "SELECT HinhAnh FROM MonAn WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value ? null : result?.ToString();
            }
        }

        public bool CapNhatDuongDanAnh(int maMon, string imagePath)
        {
            const string query = "UPDATE MonAn SET HinhAnh = @HinhAnh WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DAL.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                cmd.Parameters.AddWithValue("@HinhAnh",
                    string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);

                conn.Open();
                try { EnsureSoLuongTonColumn(conn); } catch { }
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}
