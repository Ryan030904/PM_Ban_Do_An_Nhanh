using System;
using System.Data.SqlClient;

namespace PM_Ban_Do_An_Nhanh
{
    public static class DBConnection
    {

        private static readonly string connectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=FastFoodDB;User ID=sa;Password=admin123;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            // Trả về SqlConnection mới với chuỗi kết nối cố định
            return new SqlConnection(connectionString);
        }

        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    // Nếu kết nối mở thành công thì trả về true
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        return true;
                    }

                    errorMessage = "Cannot open SQL connection.";
                    return false;
                }
            }
            catch (SqlException ex)
            {
                // Lỗi cụ thể của SQL (ví dụ: sai tên server, không kết nối được, DB không tồn tại, v.v.)
                errorMessage = "SQL error: " + ex.Message;
                return false;
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi do trạng thái kết nối hoặc chuỗi kết nối không hợp lệ
                errorMessage = "Invalid operation: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                errorMessage = "Unexpected error: " + ex.Message;
                return false;
            }
        }
    }
} 