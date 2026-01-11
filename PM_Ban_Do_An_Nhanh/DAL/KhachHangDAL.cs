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
            string query = "SELECT MaKH, TenKH, SDT, DiaChi FROM KhachHang";
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

        public DataTable LayDanhSachKhachHangTomTat()
        {
            DataTable dt = new DataTable();
            const string query = @"
                SELECT
                    kh.MaKH,
                    kh.TenKH,
                    kh.SDT,
                    COUNT(dh.MaDH) AS SoLanMua,
                    ISNULL(SUM(dh.TongTien), 0) AS TongChiTieu
                FROM KhachHang kh
                LEFT JOIN DonHang dh
                    ON dh.MaKH = kh.MaKH
                    AND dh.TrangThaiThanhToan = N'Đã thanh toán'
                GROUP BY kh.MaKH, kh.TenKH, kh.SDT
                ORDER BY kh.TenKH";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        public KhachHang LayThongTinKhachHangBySDT(string sdt)
        {
            KhachHang kh = null;
            string query = "SELECT MaKH, TenKH, SDT, DiaChi FROM KhachHang WHERE SDT = @SDT";
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
                                DiaChi = reader["DiaChi"].ToString()
                            };
                        }
                    }
                }
            }
            return kh;
        }

        public bool ThemKhachHang(KhachHang khachHang)
        {
            string query = "INSERT INTO KhachHang (TenKH, SDT, DiaChi) VALUES (@TenKH, @SDT, @DiaChi)";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)khachHang.DiaChi ?? DBNull.Value);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public bool CapNhatKhachHang(KhachHang khachHang)
        {
            string query = "UPDATE KhachHang SET TenKH = @TenKH, DiaChi = @DiaChi WHERE SDT = @SDT";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)khachHang.DiaChi ?? DBNull.Value);
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
