using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmXuatKhoUI : Form
    {
        private readonly MonAnBLL monAnBLL = new MonAnBLL();
        private readonly XuatKhoBLL xuatKhoBLL = new XuatKhoBLL();

        private readonly BindingSource chiTietSource = new BindingSource();
        private readonly List<ChiTietPhieuXuatKho> chiTietList = new List<ChiTietPhieuXuatKho>();

        private int? selectedMaPX;

        public frmXuatKhoUI()
        {
            InitializeComponent();

            chiTietSource.DataSource = chiTietList;
            dgvChiTietXuat.DataSource = chiTietSource;

            ConfigureGridBase(dgvChiTietXuat);
            ConfigureGridBase(dgvDanhSachPhieu);
            ConfigureGridBase(dgvChiTietPhieu);

            dgvChiTietXuat.DataBindingComplete += dgvChiTietXuat_DataBindingComplete;
            dgvDanhSachPhieu.DataBindingComplete += dgvDanhSachPhieu_DataBindingComplete;
            dgvChiTietPhieu.DataBindingComplete += dgvChiTietPhieu_DataBindingComplete;

            Load += frmXuatKhoUI_Load;
            btnThemDong.Click += btnThemDong_Click;
            btnLuuPhieu.Click += btnLuuPhieu_Click;
            dgvDanhSachPhieu.CellClick += dgvDanhSachPhieu_CellClick;
        }

        private void ConfigureGridBase(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.BackgroundColor = Color.White;
            dgv.GridColor = Color.Gainsboro;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        }

        private static void HideColumnIfExists(DataGridView dgv, string columnName)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].Visible = false;
            }
        }

        private static void SetHeaderIfExists(DataGridView dgv, string columnName, string header)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].HeaderText = header;
            }
        }

        private static void SetFormatIfExists(DataGridView dgv, string columnName, string format, DataGridViewContentAlignment? alignment = null)
        {
            if (!dgv.Columns.Contains(columnName)) return;
            dgv.Columns[columnName].DefaultCellStyle.Format = format;
            if (alignment.HasValue)
            {
                dgv.Columns[columnName].DefaultCellStyle.Alignment = alignment.Value;
            }
        }

        private void dgvChiTietXuat_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            HideColumnIfExists(dgvChiTietXuat, "MaPX");
            HideColumnIfExists(dgvChiTietXuat, "MaMon");

            SetHeaderIfExists(dgvChiTietXuat, "TenMon", "Món");
            SetHeaderIfExists(dgvChiTietXuat, "SoLuong", "Số lượng");
            SetHeaderIfExists(dgvChiTietXuat, "DonGia", "Đơn giá");
            SetHeaderIfExists(dgvChiTietXuat, "ThanhTien", "Thành tiền");

            SetFormatIfExists(dgvChiTietXuat, "SoLuong", "N0", DataGridViewContentAlignment.MiddleCenter);
            SetFormatIfExists(dgvChiTietXuat, "DonGia", "N0", DataGridViewContentAlignment.MiddleRight);
            SetFormatIfExists(dgvChiTietXuat, "ThanhTien", "N0", DataGridViewContentAlignment.MiddleRight);
        }

        private void dgvDanhSachPhieu_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            SetHeaderIfExists(dgvDanhSachPhieu, "MaPX", "Mã PX");
            SetHeaderIfExists(dgvDanhSachPhieu, "NgayXuat", "Ngày xuất");
            SetHeaderIfExists(dgvDanhSachPhieu, "LyDo", "Lý do");
            SetHeaderIfExists(dgvDanhSachPhieu, "GhiChu", "Ghi chú");
            SetHeaderIfExists(dgvDanhSachPhieu, "TenTK", "Tài khoản");

            SetFormatIfExists(dgvDanhSachPhieu, "NgayXuat", "dd/MM/yyyy HH:mm", DataGridViewContentAlignment.MiddleLeft);
        }

        private void dgvChiTietPhieu_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            HideColumnIfExists(dgvChiTietPhieu, "MaPX");
            HideColumnIfExists(dgvChiTietPhieu, "MaMon");

            SetHeaderIfExists(dgvChiTietPhieu, "TenMon", "Món");
            SetHeaderIfExists(dgvChiTietPhieu, "SoLuong", "Số lượng");
            SetHeaderIfExists(dgvChiTietPhieu, "DonGia", "Đơn giá");
            SetHeaderIfExists(dgvChiTietPhieu, "ThanhTien", "Thành tiền");

            SetFormatIfExists(dgvChiTietPhieu, "SoLuong", "N0", DataGridViewContentAlignment.MiddleCenter);
            SetFormatIfExists(dgvChiTietPhieu, "DonGia", "N0", DataGridViewContentAlignment.MiddleRight);
            SetFormatIfExists(dgvChiTietPhieu, "ThanhTien", "N0", DataGridViewContentAlignment.MiddleRight);
        }

        private void frmXuatKhoUI_Load(object sender, EventArgs e)
        {
            try
            {
                LoadMonAn();
                LoadDanhSachPhieu();
                ClearChiTietXuat();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được dữ liệu xuất kho: " + ex.Message);
            }
        }

        private void LoadMonAn()
        {
            DataTable dt = monAnBLL.HienThiDanhSachMonAn();
            cboMon.DataSource = dt;
            cboMon.DisplayMember = "TenMon";
            cboMon.ValueMember = "MaMon";
            cboMon.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        private void LoadDanhSachPhieu()
        {
            dgvDanhSachPhieu.DataSource = xuatKhoBLL.LayDanhSachPhieuXuat();
        }

        private void LoadChiTietPhieu(int maPX)
        {
            dgvChiTietPhieu.DataSource = xuatKhoBLL.LayChiTietPhieuXuat(maPX);
        }

        private void btnThemDong_Click(object sender, EventArgs e)
        {
            if (cboMon.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn món");
                return;
            }

            if (!int.TryParse(txtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng không hợp lệ");
                return;
            }

            decimal? donGia = null;
            if (!string.IsNullOrWhiteSpace(txtDonGia.Text))
            {
                if (!decimal.TryParse(txtDonGia.Text.Trim(), out decimal dg) || dg <= 0)
                {
                    MessageBox.Show("Đơn giá không hợp lệ");
                    return;
                }
                donGia = dg;
            }

            int maMon = Convert.ToInt32(cboMon.SelectedValue);
            string tenMon = cboMon.Text;

            var existing = chiTietList.FirstOrDefault(x => x.MaMon == maMon);
            if (existing != null)
            {
                existing.SoLuong += soLuong;
                existing.DonGia = donGia;
            }
            else
            {
                chiTietList.Add(new ChiTietPhieuXuatKho
                {
                    MaMon = maMon,
                    TenMon = tenMon,
                    SoLuong = soLuong,
                    DonGia = donGia
                });
            }

            chiTietSource.ResetBindings(false);
            txtSoLuong.Clear();
            txtDonGia.Clear();
        }

        private void btnLuuPhieu_Click(object sender, EventArgs e)
        {
            try
            {
                int maPX = xuatKhoBLL.TaoPhieuXuat(txtLyDo.Text.Trim(), txtGhiChu.Text.Trim(), chiTietList.ToList());
                MessageBox.Show($"Đã lưu phiếu xuất: {maPX}");

                LoadDanhSachPhieu();
                ClearChiTietXuat();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phiếu xuất: " + ex.Message);
            }
        }

        private void dgvDanhSachPhieu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvDanhSachPhieu.Rows[e.RowIndex];
            if (row.Cells["MaPX"].Value == null) return;

            selectedMaPX = Convert.ToInt32(row.Cells["MaPX"].Value);
            LoadChiTietPhieu(selectedMaPX.Value);
        }

        private void ClearChiTietXuat()
        {
            chiTietList.Clear();
            chiTietSource.ResetBindings(false);
            txtLyDo.Clear();
            txtGhiChu.Clear();
        }

        private void dgvChiTietXuat_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
