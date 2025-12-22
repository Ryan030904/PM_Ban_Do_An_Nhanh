using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmReport : Form
    {
        // 👉 Chuỗi kết nối SQL Server (sửa lại tên server + database nếu cần)
        private readonly string connectionString =
            @"Data Source=.;Initial Catalog=PM_Ban_Do_An_Nhanh;Integrated Security=True";

        public frmReport()
        {
            InitializeComponent();
        }

        // ===================== FIX LỖI DESIGNER =====================

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Không vẽ gì – để trống tránh lỗi
        }

        private void PrintReportPage_PrintPage(object sender, PrintPageEventArgs e)
        {
            float y = 20;
            Font fontTitle = new Font("Segoe UI", 14, FontStyle.Bold);
            Font font = new Font("Segoe UI", 10);

            e.Graphics.DrawString("BÁO CÁO DOANH THU", fontTitle, Brushes.Black, 200, y);
            y += 40;

            e.Graphics.DrawString(
                $"Từ ngày: {dtpTuNgay.Value:dd/MM/yyyy}  -  Đến ngày: {dtpDenNgay.Value:dd/MM/yyyy}",
                font, Brushes.Black, 20, y);
            y += 30;

            e.Graphics.DrawString($"Tổng doanh thu: {lblTotalRevenue.Text}",
                font, Brushes.Black, 20, y);
        }

        // ===================== BUTTON EVENTS =====================

        private void btnXemBaoCao_Click(object sender, EventArgs e)
        {
            LoadDoanhThu();
            LoadMonBanChay();
        }

        private void btnInBaoCao_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = PrintReportPage;
            preview.Width = 1000;
            preview.Height = 700;
            preview.ShowDialog();
        }

        // ===================== LOAD DATA =====================

        private void LoadDoanhThu()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"
                    SELECT 
                        CAST(NgayLap AS DATE) AS Ngay,
                        SUM(TongTien) AS DoanhThuNgay
                    FROM HoaDon
                    WHERE (@TuNgay IS NULL OR NgayLap >= @TuNgay)
                      AND (@DenNgay IS NULL OR NgayLap <= @DenNgay)
                    GROUP BY CAST(NgayLap AS DATE)
                    ORDER BY Ngay";

                cmd.Parameters.AddWithValue("@TuNgay",
                    dtpTuNgay.Checked ? (object)dtpTuNgay.Value.Date : DBNull.Value);

                cmd.Parameters.AddWithValue("@DenNgay",
                    dtpDenNgay.Checked ? (object)dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1) : DBNull.Value);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                dgvDoanhThu.DataSource = dt;

                // Tính tổng doanh thu
                decimal total = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row["DoanhThuNgay"] != DBNull.Value)
                        total += Convert.ToDecimal(row["DoanhThuNgay"]);
                }

                lblTotalRevenue.Text = total.ToString("N0") + " VNĐ";
            }
        }

        private void LoadMonBanChay()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = @"
                    SELECT 
                        m.TenMon,
                        SUM(ct.SoLuong) AS TongSoLuongBan,
                        SUM(ct.ThanhTien) AS TongDoanhThuMon
                    FROM ChiTietHoaDon ct
                    JOIN MonAn m ON ct.MaMon = m.MaMon
                    JOIN HoaDon h ON ct.MaHoaDon = h.MaHoaDon
                    WHERE (@TuNgay IS NULL OR h.NgayLap >= @TuNgay)
                      AND (@DenNgay IS NULL OR h.NgayLap <= @DenNgay)
                    GROUP BY m.TenMon
                    ORDER BY TongSoLuongBan DESC";

                cmd.Parameters.AddWithValue("@TuNgay",
                    dtpTuNgay.Checked ? (object)dtpTuNgay.Value.Date : DBNull.Value);

                cmd.Parameters.AddWithValue("@DenNgay",
                    dtpDenNgay.Checked ? (object)dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1) : DBNull.Value);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                dgvMonBanChay.DataSource = dt;
            }
        }
    }
}
