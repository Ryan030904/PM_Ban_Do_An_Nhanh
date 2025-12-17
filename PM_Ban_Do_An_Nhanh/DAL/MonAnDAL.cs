using System;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class MonAnDAL
    {
        public DataTable LayDanhSachMonAn()
        {
            DataTable dt = new DataTable();
            string query = "SELECT MaMon, TenMon, Gia, M.MaDM, TenDM, TrangThai, HinhAnh " +
                           "FROM MonAn M JOIN DanhMuc DM ON M.MaDM = DM.MaDM";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                conn.Open();
                da.Fill(dt);
            }

            return dt;
        }

        public int ThemMonAn(MonAn monAn)
        {
            const string query =
                "INSERT INTO MonAn (TenMon, Gia, MaDM, TrangThai, HinhAnh) " +
                "VALUES (@TenMon, @Gia, @MaDM, @TrangThai, @HinhAnh); " +
                "SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TenMon", monAn.TenMon);
                cmd.Parameters.AddWithValue("@Gia", monAn.Gia);
                cmd.Parameters.AddWithValue("@MaDM", monAn.MaDM);
                cmd.Parameters.AddWithValue("@TrangThai", monAn.TrangThai);
                cmd.Parameters.AddWithValue("@HinhAnh",
                    string.IsNullOrEmpty(monAn.HinhAnh) ? (object)DBNull.Value : monAn.HinhAnh);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public bool SuaMonAn(MonAn monAn)
        {
            const string query =
                "UPDATE MonAn SET TenMon = @TenMon, Gia = @Gia, MaDM = @MaDM, " +
                "TrangThai = @TrangThai, HinhAnh = @HinhAnh WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
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
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool XoaMonAn(int maMon)
        {
            const string query = "DELETE FROM MonAn WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public string LayDuongDanAnhByMaMon(int maMon)
        {
            const string query = "SELECT HinhAnh FROM MonAn WHERE MaMon = @MaMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
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

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                cmd.Parameters.AddWithValue("@HinhAnh",
                    string.IsNullOrEmpty(imagePath) ? (object)DBNull.Value : imagePath);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}

namespace PM_Ban_Do_An_Nhanh
{
    public static class DBConnection
    {
        public static SqlConnection GetConnection() { ... }
    }
}
