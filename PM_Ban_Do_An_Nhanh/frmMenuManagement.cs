using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Helpers;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmMenuManagement : Form
    {
        private readonly MonAnBLL monAnBLL = new MonAnBLL();
        private readonly DanhMucBLL danhMucBLL = new DanhMucBLL();

        private int selectedMonAnId = -1;
        private string selectedImagePath = "";

        public frmMenuManagement()
        {
            InitializeComponent();

            this.Text = "🍽️ Quản lý thực đơn";

            // GẮN EVENT
            dgvMonAn.CellClick += dgvMonAn_CellClick;
            btnThem.Click += btnThem_Click;
            btnSua.Click += btnSua_Click;
            btnXoa.Click += btnXoa_Click;
            btnLamMoi.Click += btnLamMoi_Click;
            btnChonHinh.Click += btnChonHinh_Click;

            SetupTrangThaiComboBox();
            LoadDanhMucToComboBox();
            LoadDataToDataGridView();
            ClearInputFields();
            SetupButtonStyles();
        }

        // ======================= STYLE =======================
        private void SetupButtonStyles()
        {
            ButtonStyleHelper.ApplySuccessStyle(btnThem, "➕ Thêm", "", ButtonSize.Medium);
            ButtonStyleHelper.ApplyWarningStyle(btnSua, "✏️ Sửa", "", ButtonSize.Medium);
            ButtonStyleHelper.ApplyDangerStyle(btnXoa, "🗑️ Xóa", "", ButtonSize.Medium);
            ButtonStyleHelper.ApplyInfoStyle(btnLamMoi, "🔄 Làm mới", "", ButtonSize.Medium);
            ButtonStyleHelper.ApplyPrimaryStyle(btnChonHinh, "🖼️ Chọn hình", "", ButtonSize.Medium);

            picHinhAnh.SizeMode = PictureBoxSizeMode.Zoom;
            picHinhAnh.BorderStyle = BorderStyle.FixedSingle;
        }

        // ======================= LOAD DATA =======================
        private void LoadDataToDataGridView()
        {
            dgvMonAn.DataSource = monAnBLL.HienThiDanhSachMonAn();
            dgvMonAn.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvMonAn.Columns["MaDM"].Visible = false;
            dgvMonAn.Columns["HinhAnh"].Visible = false;
        }

        private void LoadDanhMucToComboBox()
        {
            DataTable dt = danhMucBLL.LayDanhSachDanhMuc();
            cboDanhMuc.DataSource = dt;
            cboDanhMuc.DisplayMember = "TenDM";
            cboDanhMuc.ValueMember = "MaDM";
            cboDanhMuc.SelectedIndex = -1;
        }

        private void SetupTrangThaiComboBox()
        {
            cboTrangThai.Items.Clear();
            cboTrangThai.Items.Add("Còn hàng");
            cboTrangThai.Items.Add("Hết hàng");
            cboTrangThai.SelectedIndex = 0;
        }

        // ======================= GRID CLICK =======================
        private void dgvMonAn_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvMonAn.Rows[e.RowIndex];

            if (row.Cells["MaMon"].Value == DBNull.Value) return;

            selectedMonAnId = Convert.ToInt32(row.Cells["MaMon"].Value);
            txtTenMon.Text = row.Cells["TenMon"].Value?.ToString() ?? "";
            txtGia.Text = row.Cells["Gia"].Value?.ToString() ?? "";
            cboDanhMuc.SelectedValue = row.Cells["MaDM"].Value;
            cboTrangThai.SelectedItem = row.Cells["TrangThai"].Value?.ToString();

            string img = row.Cells["HinhAnh"].Value == DBNull.Value ? "" : row.Cells["HinhAnh"].Value.ToString();
            LoadImage(img);
        }

        private void LoadImage(string path)
        {
            picHinhAnh.Image?.Dispose();
            picHinhAnh.Image = ImageHelper.LoadMenuItemImage(path);
        }

        // ======================= BUTTON EVENTS =======================
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            monAnBLL.ThemMonAn(
                txtTenMon.Text,
                Convert.ToDecimal(txtGia.Text),
                (int)cboDanhMuc.SelectedValue,
                cboTrangThai.Text,
                selectedImagePath
            );

            LoadDataToDataGridView();
            ClearInputFields();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (selectedMonAnId == -1)
            {
                MessageBox.Show("Chưa chọn món cần sửa");
                return;
            }

            if (!ValidateInput()) return;

            monAnBLL.SuaMonAn(
                selectedMonAnId,
                txtTenMon.Text,
                Convert.ToDecimal(txtGia.Text),
                (int)cboDanhMuc.SelectedValue,
                cboTrangThai.Text,
                selectedImagePath
            );

            LoadDataToDataGridView();
            ClearInputFields();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (selectedMonAnId == -1)
            {
                MessageBox.Show("Chưa chọn món cần xóa");
                return;
            }

            if (MessageBox.Show("Xác nhận xóa?", "Xóa", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                monAnBLL.XoaMonAn(selectedMonAnId);
                LoadDataToDataGridView();
                ClearInputFields();
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ClearInputFields();
            LoadDataToDataGridView();
        }

        private void btnChonHinh_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image|*.jpg;*.png;*.jpeg";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = dlg.FileName;
                picHinhAnh.Image?.Dispose();
                picHinhAnh.Image = Image.FromFile(selectedImagePath);
            }

            dlg.Dispose();
        }

        // ======================= VALIDATE =======================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTenMon.Text)) return false;
            if (!decimal.TryParse(txtGia.Text, out decimal g) || g <= 0) return false;
            if (cboDanhMuc.SelectedValue == null) return false;
            return true;
        }

        private void ClearInputFields()
        {
            selectedMonAnId = -1;
            selectedImagePath = "";
            txtTenMon.Clear();
            txtGia.Clear();
            cboDanhMuc.SelectedIndex = -1;
            cboTrangThai.SelectedIndex = 0;
            picHinhAnh.Image?.Dispose();
            picHinhAnh.Image = null;
        }
    }
}
