using System;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class KhachHangDAL
    {
        public DataTable LayDanhSachKhachHang()
        {
            DataTable dt = new DataTable();
            string query = "SELECT MaKH, TenKH, SDT, DiaChi, Email, NgaySinh FROM KhachHang";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        public KhachHang LayThongTinKhachHangBySDT(string sdt)
        {
            KhachHang kh = null;
            string query = "SELECT MaKH, TenKH, SDT, DiaChi, Email, NgaySinh FROM KhachHang WHERE SDT = @SDT";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SDT", sdt);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            kh = new KhachHang
                            {
                                MaKH = Convert.ToInt32(reader["MaKH"]),
                                TenKH = reader["TenKH"].ToString(),
                                SDT = reader["SDT"].ToString(),
                                DiaChi = reader["DiaChi"].ToString(),
                                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                                NgaySinh = reader["NgaySinh"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["NgaySinh"])
                            };
                        }
                    }
                }
            }
            return kh;
        }

        public bool ThemKhachHang(KhachHang khachHang)
        {
            string query = "INSERT INTO KhachHang (TenKH, SDT, DiaChi, Email, NgaySinh) VALUES (@TenKH, @SDT, @DiaChi, @Email, @NgaySinh)";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)khachHang.DiaChi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)khachHang.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NgaySinh", (object)khachHang.NgaySinh ?? DBNull.Value);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public bool CapNhatKhachHang(KhachHang khachHang)
        {
            string query = "UPDATE KhachHang SET TenKH = @TenKH, DiaChi = @DiaChi, Email = @Email, NgaySinh = @NgaySinh WHERE SDT = @SDT";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)khachHang.DiaChi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)khachHang.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NgaySinh", (object)khachHang.NgaySinh ?? DBNull.Value);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool XoaKhachHang(string sdt)
        {
            string query = "DELETE FROM KhachHang WHERE SDT = @SDT";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SDT", sdt);
                    conn.Open();

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            throw new Exception("Không thể xóa khách hàng này vì đang được tham chiếu trong đơn hàng.");
                        }
                        throw;
                    }
                }
            }
        }
    }
}
