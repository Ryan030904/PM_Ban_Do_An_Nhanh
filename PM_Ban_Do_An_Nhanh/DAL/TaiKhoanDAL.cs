using PM_Ban_Do_An_Nhanh.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class TaiKhoanDAL
    {
        public TaiKhoan KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            TaiKhoan tk = null;
            string query = "SELECT MaTK, TenTK, TenDangNhap, MatKhau FROM TaiKhoan WHERE TenDangNhap = @TenDangNhap AND MatKhau = @MatKhau";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", matKhau);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tk = new TaiKhoan
                            {
                                MaTK = Convert.ToInt32(reader["MaTK"]),
                                TenTK = reader["TenTK"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString()
                            };
                        }
                    }
                }
            }
            return tk;
        }
    }
}