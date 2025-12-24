using System;
using System.Data;
using System.Data.SqlClient;

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public class TonKhoDAL
    {
        public DataTable LayTonKhoMonAn()
        {
            var dt = new DataTable();
            const string query = @"
                SELECT tk.MaMon, ma.TenMon, tk.SoLuongTon, tk.NgayCapNhat
                FROM TonKhoMonAn tk
                JOIN MonAn ma ON ma.MaMon = tk.MaMon
                ORDER BY ma.TenMon";

            using (SqlConnection conn = PM_Ban_Do_An_Nhanh.DBConnection.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                conn.Open();
                da.Fill(dt);
            }

            return dt;
        }

        public int LaySoLuongTon(int maMon, SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"SELECT SoLuongTon FROM TonKhoMonAn WITH (UPDLOCK, HOLDLOCK) WHERE MaMon = @MaMon";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return 0;
                return Convert.ToInt32(result);
            }
        }

        public void CongTon(int maMon, int soLuong, SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                IF EXISTS (SELECT 1 FROM TonKhoMonAn WHERE MaMon = @MaMon)
                    UPDATE TonKhoMonAn
                    SET SoLuongTon = SoLuongTon + @SoLuong,
                        NgayCapNhat = GETDATE()
                    WHERE MaMon = @MaMon
                ELSE
                    INSERT INTO TonKhoMonAn (MaMon, SoLuongTon, NgayCapNhat)
                    VALUES (@MaMon, @SoLuong, GETDATE());";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                cmd.ExecuteNonQuery();
            }
        }

        public void TruTon(int maMon, int soLuong, SqlConnection conn, SqlTransaction transaction)
        {
            int ton = LaySoLuongTon(maMon, conn, transaction);
            if (ton < soLuong)
            {
                throw new InvalidOperationException($"Tồn kho không đủ cho món (MaMon={maMon}). Tồn={ton}, yêu cầu={soLuong}");
            }

            const string query = @"
                UPDATE TonKhoMonAn
                SET SoLuongTon = SoLuongTon - @SoLuong,
                    NgayCapNhat = GETDATE()
                WHERE MaMon = @MaMon";

            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@MaMon", maMon);
                cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
