using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.UI;

namespace PM_Ban_Do_An_Nhanh
{
    public class frmKhuyenMai : Form
    {
        private DataGridView dgv;
        private Label lblTitle;

        public frmKhuyenMai()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Khuyến mãi";
            this.BackColor = Color.White;

            lblTitle = new Label();
            lblTitle.Text = "Bảng khuyến mãi theo hạng thành viên";
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 44;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Padding = new Padding(12, 0, 12, 0);

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.Enabled = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.TabStop = true;
            dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;

            dgv.CellClick += dgv_CellClick;
            dgv.SelectionChanged += dgv_SelectionChanged;

            this.Controls.Add(dgv);
            this.Controls.Add(lblTitle);
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            try
            {
                dgv.ClearSelection();
                dgv.CurrentCell = null;
            }
            catch
            {
            }
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                dgv.ClearSelection();
                dgv.CurrentCell = null;
            }
            catch
            {
            }
        }

        private void LoadData()
        {
            var dt = new DataTable();
            dt.Columns.Add("Hang", typeof(string));
            dt.Columns.Add("DieuKien", typeof(string));
            dt.Columns.Add("GiamGia", typeof(string));

            dt.Rows.Add("Thành Viên (Member)", "0 - 500.000", "2%");
            dt.Rows.Add("Bạc (Silver)", "> 500.000", "5%");
            dt.Rows.Add("Vàng (Gold)", "> 2.000.000", "8%");
            dt.Rows.Add("Bạch Kim (Platinum)", "> 5.000.000", "12%");
            dt.Rows.Add("Kim Cương (Diamond)", "> 8.000.000", "15%");

            dgv.DataSource = dt;

            if (dgv.Columns.Contains("Hang")) dgv.Columns["Hang"].HeaderText = "Hạng";
            if (dgv.Columns.Contains("DieuKien")) dgv.Columns["DieuKien"].HeaderText = "Điều kiện (Tổng đã chi)";
            if (dgv.Columns.Contains("GiamGia")) dgv.Columns["GiamGia"].HeaderText = "Giảm giá";

            TableStyleHelper.ApplyModernStyle(dgv, "primary");

            // View-only: disable any visible selection/highlight (still allow scroll)
            dgv.ClearSelection();
            dgv.CurrentCell = null;
            dgv.DefaultCellStyle.SelectionBackColor = dgv.DefaultCellStyle.BackColor;
            dgv.DefaultCellStyle.SelectionForeColor = dgv.DefaultCellStyle.ForeColor;
            dgv.RowTemplate.DefaultCellStyle.SelectionBackColor = dgv.DefaultCellStyle.BackColor;
            dgv.RowTemplate.DefaultCellStyle.SelectionForeColor = dgv.DefaultCellStyle.ForeColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;
        }
    }
}
