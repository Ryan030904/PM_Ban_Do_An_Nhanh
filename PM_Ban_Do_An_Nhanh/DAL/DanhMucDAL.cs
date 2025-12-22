using System;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class DanhMucDAL
    {
        public DataTable LayDanhSachDanhMuc()
        {
            var dt = new DataTable();
            const string query = "SELECT MaDM, TenDM FROM DanhMuc";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandTimeout = 5;

                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                catch (SqlException ex)
                {
                    throw new Exception("Lỗi khi tải danh sách khách hàng: " + ex.Message, ex);
                }
            }

            return dt;
        }

        public bool ThemDanhMuc(string tenDM)
        {
            const string query = "INSERT INTO DanhMuc (TenDM) VALUES (@TenDM)";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TenDM", tenDM ?? (object)DBNull.Value);
                cmd.CommandTimeout = 5;

                try
                {
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        public bool SuaDanhMuc(int maDM, string tenDM)
        {
            const string query = "UPDATE DanhMuc SET TenDM = @TenDM WHERE MaDM = @MaDM";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@TenDM", tenDM ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaDM", maDM);
                cmd.CommandTimeout = 5;

                try
                {
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        public bool XoaDanhMuc(int maDM)
        {
            const string query = "DELETE FROM DanhMuc WHERE MaDM = @MaDM";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaDM", maDM);
                cmd.CommandTimeout = 5;

                try
                {
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }
    }
}