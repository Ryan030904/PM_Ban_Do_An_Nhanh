using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.UI;
using PM_Ban_Do_An_Nhanh.Utils;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmDanhMuc : Form
    {
        private DanhMucBLL danhMucBLL = new DanhMucBLL();
        private int selectedMaDM = -1;

        public frmDanhMuc()
        {
            InitializeComponent();
            this.Text = "Quản lý danh mục";

            SetupButtonStyles();

            // ✅ FIX QUAN TRỌNG – GẮN EVENT
            this.Load += frmDanhMuc_Load;
            dgvDanhMuc.CellClick += dgvDanhMuc_CellClick;
        }

        private void SetupButtonStyles()
        {
            btnThem.Text = "Thêm";
            btnSua.Text = "Sửa";
            btnXoa.Text = "Xóa";
        }

        private void frmDanhMuc_Load(object sender, EventArgs e)
        {
            LoadDataToDataGridView();
            ClearInputFields();
        }

        private void LoadDataToDataGridView()
        {
            try
            {
                dgvDanhMuc.DataSource = danhMucBLL.LayDanhSachDanhMuc();
                dgvDanhMuc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                TableStyleHelper.DisableSorting(dgvDanhMuc);

                if (dgvDanhMuc.Columns["MaDM"] != null)
                    dgvDanhMuc.Columns["MaDM"].HeaderText = "Mã Danh Mục";

                if (dgvDanhMuc.Columns["TenDM"] != null)
                    dgvDanhMuc.Columns["TenDM"].HeaderText = "Tên Danh Mục";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu danh mục: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvDanhMuc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDanhMuc.Rows[e.RowIndex];

                txtMaDanhMuc.Text = row.Cells["MaDM"].Value?.ToString();
                txtTenDanhMuc.Text = row.Cells["TenDM"].Value?.ToString();

                if (int.TryParse(txtMaDanhMuc.Text, out int maDM))
                    selectedMaDM = maDM;
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenDanhMuc.Text))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool result = danhMucBLL.ThemDanhMuc(txtTenDanhMuc.Text.Trim());

                if (result)
                {
                    MessageBox.Show("Thêm danh mục thành công!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataToDataGridView();
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Thêm danh mục thất bại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm danh mục: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (selectedMaDM == -1)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần sửa!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool result = danhMucBLL.SuaDanhMuc(
                    selectedMaDM,
                    txtTenDanhMuc.Text.Trim()
                );

                if (result)
                {
                    MessageBox.Show("Sửa danh mục thành công!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataToDataGridView();
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Sửa danh mục thất bại!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa danh mục: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (selectedMaDM == -1)
            {
                MessageBox.Show("Vui lòng chọn danh mục cần xóa!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa danh mục '{txtTenDanhMuc.Text}'?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool result = danhMucBLL.XoaDanhMuc(selectedMaDM);

                    if (result)
                    {
                        MessageBox.Show("Xóa danh mục thành công!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataToDataGridView();
                        ClearInputFields();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa danh mục đang được sử dụng!",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa danh mục: " + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearInputFields()
        {
            txtMaDanhMuc.Text = "";
            txtTenDanhMuc.Text = "";
            selectedMaDM = -1;
        }

        private void dgvDanhMuc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
