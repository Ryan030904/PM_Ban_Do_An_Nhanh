using System;
using System.Data;
using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class KhachHangDAL
    {
        private static bool IsInvalidColumn(SqlException ex, string columnName)
        {
            try
            {
                return ex != null
                    && ex.Number == 207
                    && ex.Message != null
                    && ex.Message.IndexOf(columnName, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static DataTable FillDataTable(string query, Action<SqlCommand> bind)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                bind?.Invoke(cmd);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        private static DataTable FillDataTableWithFallback(string queryPrimary, string queryFallback, string columnName, Action<SqlCommand> bind)
        {
            try
            {
                return FillDataTable(queryPrimary, bind);
            }
            catch (SqlException ex) when (IsInvalidColumn(ex, columnName))
            {
                return FillDataTable(queryFallback, bind);
            }
        }

        private static int ExecuteNonQuery(string query, Action<SqlCommand> bind)
        {
            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                bind?.Invoke(cmd);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private static int ExecuteNonQueryWithFallback(string queryPrimary, string queryFallback, string columnName, Action<SqlCommand> bind)
        {
            try
            {
                return ExecuteNonQuery(queryPrimary, bind);
            }
            catch (SqlException ex) when (IsInvalidColumn(ex, columnName))
            {
                return ExecuteNonQuery(queryFallback, bind);
            }
        }

        public DataTable LayDanhSachKhachHang()
        {
            const string queryPrimary = "SELECT MaKH, TenKH, SDT, DiaChi, TrangThai FROM KhachHang";
            const string queryFallback = "SELECT MaKH, TenKH, SDT, DiaChi FROM KhachHang";
            return FillDataTableWithFallback(queryPrimary, queryFallback, "TrangThai", null);
        }

        public DataTable LayDanhSachKhachHangByTen(string tenKH, string mode)
        {
            string where = " WHERE LOWER(LTRIM(RTRIM(kh.TenKH))) = LOWER(LTRIM(RTRIM(@TenKH)))";
            if (string.Equals(mode, "Active", StringComparison.OrdinalIgnoreCase))
                where += " AND (kh.TrangThai IS NULL OR kh.TrangThai = 'Active')";
            else if (string.Equals(mode, "Inactive", StringComparison.OrdinalIgnoreCase))
                where += " AND kh.TrangThai = 'Inactive'";

            string queryPrimary = @"
SELECT kh.MaKH, kh.TenKH, kh.SDT, kh.DiaChi, kh.TrangThai
FROM KhachHang kh" + where;

            const string queryFallback = @"
SELECT kh.MaKH, kh.TenKH, kh.SDT, kh.DiaChi
FROM KhachHang kh
WHERE LOWER(LTRIM(RTRIM(kh.TenKH))) = LOWER(LTRIM(RTRIM(@TenKH)))";

            return FillDataTableWithFallback(
                queryPrimary,
                queryFallback,
                "TrangThai",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@TenKH", tenKH);
                });
        }

        public DataTable LayDanhSachKhachHangTheoTrangThai(string mode)
        {
            string where = "";
            if (string.Equals(mode, "Active", StringComparison.OrdinalIgnoreCase))
                where = " WHERE (TrangThai IS NULL OR TrangThai = 'Active')";
            else if (string.Equals(mode, "Inactive", StringComparison.OrdinalIgnoreCase))
                where = " WHERE TrangThai = 'Inactive'";

            string queryPrimary = "SELECT MaKH, TenKH, SDT, DiaChi, TrangThai FROM KhachHang" + where + " ORDER BY TenKH";
            string queryFallback = "SELECT MaKH, TenKH, SDT, DiaChi FROM KhachHang ORDER BY TenKH";
            return FillDataTableWithFallback(queryPrimary, queryFallback, "TrangThai", null);
        }

        public DataTable LayDanhSachKhachHangTomTat()
        {
            return LayDanhSachKhachHangTomTatTheoTrangThai("All");
        }

        public DataTable LayDanhSachKhachHangTomTatTheoTrangThai(string mode)
        {
            string where = "";
            if (string.Equals(mode, "Active", StringComparison.OrdinalIgnoreCase))
                where = " WHERE (kh.TrangThai IS NULL OR kh.TrangThai = 'Active')";
            else if (string.Equals(mode, "Inactive", StringComparison.OrdinalIgnoreCase))
                where = " WHERE kh.TrangThai = 'Inactive'";

            string queryPrimary = @"
                SELECT
                    kh.MaKH,
                    kh.TenKH,
                    kh.SDT,
                    kh.TrangThai,
                    COUNT(dh.MaDH) AS SoLanMua,
                    ISNULL(SUM(dh.TongTien), 0) AS TongChiTieu
                FROM KhachHang kh
                LEFT JOIN DonHang dh
                    ON dh.MaKH = kh.MaKH
                    AND dh.TrangThaiThanhToan = N'Đã thanh toán'" + where + @"
                GROUP BY kh.MaKH, kh.TenKH, kh.SDT, kh.TrangThai
                ORDER BY kh.TenKH";

            const string queryFallback = @"
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

            return FillDataTableWithFallback(queryPrimary, queryFallback, "TrangThai", null);
        }

        public KhachHang LayThongTinKhachHangBySDT(string sdt)
        {
            KhachHang kh = null;
            const string queryPrimary = "SELECT MaKH, TenKH, SDT, DiaChi, TrangThai FROM KhachHang WHERE SDT = @SDT";
            const string queryFallback = "SELECT MaKH, TenKH, SDT, DiaChi FROM KhachHang WHERE SDT = @SDT";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(queryPrimary, conn))
            {
                cmd.Parameters.AddWithValue("@SDT", sdt);
                try
                {
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
                                TrangThai = reader["TrangThai"]?.ToString()
                            };
                        }
                    }
                }
                catch (SqlException ex) when (IsInvalidColumn(ex, "TrangThai"))
                {
                    cmd.CommandText = queryFallback;
                    cmd.Connection.Close();
                    cmd.Connection.Open();
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
                                TrangThai = null
                            };
                        }
                    }
                }
            }
            return kh;
        }

        public bool ThemKhachHang(KhachHang khachHang)
        {
            const string queryPrimary = "INSERT INTO KhachHang (TenKH, SDT, DiaChi, TrangThai) VALUES (@TenKH, @SDT, @DiaChi, @TrangThai)";
            const string queryFallback = "INSERT INTO KhachHang (TenKH, SDT, DiaChi) VALUES (@TenKH, @SDT, @DiaChi)";

            int rowsAffected = ExecuteNonQueryWithFallback(
                queryPrimary,
                queryFallback,
                "TrangThai",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)khachHang.DiaChi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TrangThai", (object)(khachHang.TrangThai ?? "Active"));
                });

            return rowsAffected > 0;
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

        public bool CapNhatKhachHangTheoSdtCu(KhachHang khachHang, string sdtCu)
        {
            string query = "UPDATE KhachHang SET TenKH = @TenKH, SDT = @SDT_Moi, DiaChi = @DiaChi WHERE SDT = @SDT_Cu";
            using (SqlConnection conn = DBConnection.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenKH", khachHang.TenKH);
                    cmd.Parameters.AddWithValue("@SDT_Moi", khachHang.SDT);
                    cmd.Parameters.AddWithValue("@SDT_Cu", sdtCu);
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

        public bool CapNhatTrangThaiKhachHang(string sdt, string trangThai)
        {
            const string queryPrimary = "UPDATE KhachHang SET TrangThai = @TrangThai WHERE SDT = @SDT";
            const string queryFallback = "UPDATE KhachHang SET SDT = SDT WHERE SDT = @SDT";

            int rowsAffected = ExecuteNonQueryWithFallback(
                queryPrimary,
                queryFallback,
                "TrangThai",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@SDT", sdt);
                    cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                });

            if (rowsAffected <= 0) return false;

            // If we hit fallback, it still returns 1 row, but it didn't really update status.
            // Detect via not having TrangThai column by attempting a lightweight query would be expensive here.
            // So we treat fallback as unsupported when column doesn't exist by requiring the primary update.
            // The ExecuteNonQueryWithFallback can't distinguish, so we re-check by fetching the customer.
            try
            {
                var kh = LayThongTinKhachHangBySDT(sdt);
                if (kh != null && kh.TrangThai != null)
                    return true;
            }
            catch { }

            return false;
        }

        public decimal LayTongChiTieuBySDT(string sdt)
        {
            const string query = @"
SELECT ISNULL(SUM(dh.TongTien), 0)
FROM DonHang dh
INNER JOIN KhachHang kh ON dh.MaKH = kh.MaKH
WHERE kh.SDT = @SDT
  AND dh.TrangThaiThanhToan = N'Đã thanh toán'";

            using (SqlConnection conn = DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SDT", sdt);
                conn.Open();
                object val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value) return 0m;
                try { return Convert.ToDecimal(val); } catch { return 0m; }
            }
        }
    }
}
