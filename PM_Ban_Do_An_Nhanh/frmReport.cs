using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.UI;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmReport : Form
    {
        private readonly string connectionString = DBConnection.GetConnection().ConnectionString;
        private readonly DonHangBLL donHangBLL = new DonHangBLL();

        public frmReport()
        {
            InitializeComponent();

            this.Load += frmReport_Load;
        }

        private void frmReport_Load(object sender, EventArgs e)
        {
            try
            {
                if (dgvDoanhThu != null)
                {
                    dgvDoanhThu.AllowUserToAddRows = false;
                    dgvDoanhThu.AllowUserToDeleteRows = false;
                    dgvDoanhThu.ReadOnly = true;
                    dgvDoanhThu.MultiSelect = false;
                    dgvDoanhThu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvDoanhThu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

                if (dgvMonBanChay != null)
                {
                    dgvMonBanChay.AllowUserToAddRows = false;
                    dgvMonBanChay.AllowUserToDeleteRows = false;
                    dgvMonBanChay.ReadOnly = true;
                    dgvMonBanChay.MultiSelect = false;
                    dgvMonBanChay.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvMonBanChay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch
            {
            }

            try
            {
                LoadDoanhThu();
                LoadMonBanChay();
            }
            catch
            {
            }
        }

        private void NormalizeDateRange(ref DateTime? tuNgay, ref DateTime? denNgay)
        {
            if (!tuNgay.HasValue) return;
            if (!denNgay.HasValue) return;

            if (tuNgay.Value.Date > denNgay.Value.Date)
            {
                var tmp = tuNgay;
                tuNgay = denNgay;
                denNgay = tmp;

                try
                {
                    if (dtpTuNgay != null)
                    {
                        dtpTuNgay.Checked = true;
                        dtpTuNgay.Value = tuNgay.Value;
                    }
                    if (dtpDenNgay != null)
                    {
                        dtpDenNgay.Checked = true;
                        dtpDenNgay.Value = denNgay.Value;
                    }
                }
                catch
                {
                }
            }
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
            DateTime? tuNgay = dtpTuNgay.Checked ? dtpTuNgay.Value.Date : (DateTime?)null;
            DateTime? denNgay = dtpDenNgay.Checked ? dtpDenNgay.Value.Date : (DateTime?)null;

            NormalizeDateRange(ref tuNgay, ref denNgay);

            DataTable dt = donHangBLL.LayThongKeDoanhThu(tuNgay, denNgay) ?? new DataTable();
            dgvDoanhThu.DataSource = dt;

            // VNĐ formatting for revenue column
            TableStyleHelper.ApplyVndFormatting(dgvDoanhThu, "DoanhThuNgay");

            // Tính tổng doanh thu
            decimal total = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (dt.Columns.Contains("DoanhThuNgay") && row["DoanhThuNgay"] != DBNull.Value)
                    total += Convert.ToDecimal(row["DoanhThuNgay"]);
            }

            lblTotalRevenue.Text = TableStyleHelper.FormatVnd(total);
        }

        private void LoadMonBanChay()
        {
            DateTime? tuNgay = dtpTuNgay.Checked ? dtpTuNgay.Value.Date : (DateTime?)null;
            DateTime? denNgay = dtpDenNgay.Checked ? dtpDenNgay.Value.Date : (DateTime?)null;

            NormalizeDateRange(ref tuNgay, ref denNgay);

            DataTable dt = donHangBLL.LayMonAnBanChay(tuNgay, denNgay, 10) ?? new DataTable();
            dgvMonBanChay.DataSource = dt;

            // VNĐ formatting for total revenue per item
            TableStyleHelper.ApplyVndFormatting(dgvMonBanChay, "TongDoanhThuMon");
        }
    }
}
