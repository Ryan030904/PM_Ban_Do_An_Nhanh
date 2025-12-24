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
    public partial class frmNhapKhoUI : Form
    {
        private readonly MonAnBLL monAnBLL = new MonAnBLL();
        private readonly NhapKhoBLL nhapKhoBLL = new NhapKhoBLL();

        private readonly BindingSource chiTietSource = new BindingSource();
        private readonly List<ChiTietPhieuNhapKho> chiTietList = new List<ChiTietPhieuNhapKho>();

        private int? selectedMaPN;

        public frmNhapKhoUI()
        {
            InitializeComponent();

            chiTietSource.DataSource = chiTietList;
            dgvChiTietNhap.DataSource = chiTietSource;

            ConfigureGridBase(dgvChiTietNhap);
            ConfigureGridBase(dgvDanhSachPhieu);
            ConfigureGridBase(dgvChiTietPhieu);

            dgvChiTietNhap.DataBindingComplete += dgvChiTietNhap_DataBindingComplete;
            dgvDanhSachPhieu.DataBindingComplete += dgvDanhSachPhieu_DataBindingComplete;
            dgvChiTietPhieu.DataBindingComplete += dgvChiTietPhieu_DataBindingComplete;

            Load += frmNhapKhoUI_Load;
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

        private void dgvChiTietNhap_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            HideColumnIfExists(dgvChiTietNhap, "MaPN");
            HideColumnIfExists(dgvChiTietNhap, "MaMon");

            SetHeaderIfExists(dgvChiTietNhap, "TenMon", "Món");
            SetHeaderIfExists(dgvChiTietNhap, "SoLuong", "Số lượng");
            SetHeaderIfExists(dgvChiTietNhap, "DonGia", "Đơn giá");
            SetHeaderIfExists(dgvChiTietNhap, "ThanhTien", "Thành tiền");

            SetFormatIfExists(dgvChiTietNhap, "SoLuong", "N0", DataGridViewContentAlignment.MiddleCenter);
            SetFormatIfExists(dgvChiTietNhap, "DonGia", "N0", DataGridViewContentAlignment.MiddleRight);
            SetFormatIfExists(dgvChiTietNhap, "ThanhTien", "N0", DataGridViewContentAlignment.MiddleRight);
        }

        private void dgvDanhSachPhieu_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            SetHeaderIfExists(dgvDanhSachPhieu, "MaPN", "Mã PN");
            SetHeaderIfExists(dgvDanhSachPhieu, "NgayNhap", "Ngày nhập");
            SetHeaderIfExists(dgvDanhSachPhieu, "TongTien", "Tổng tiền");
            SetHeaderIfExists(dgvDanhSachPhieu, "GhiChu", "Ghi chú");
            SetHeaderIfExists(dgvDanhSachPhieu, "TenTK", "Tài khoản");

            SetFormatIfExists(dgvDanhSachPhieu, "NgayNhap", "dd/MM/yyyy HH:mm", DataGridViewContentAlignment.MiddleLeft);
            SetFormatIfExists(dgvDanhSachPhieu, "TongTien", "N0", DataGridViewContentAlignment.MiddleRight);
        }

        private void dgvChiTietPhieu_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            HideColumnIfExists(dgvChiTietPhieu, "MaPN");
            HideColumnIfExists(dgvChiTietPhieu, "MaMon");

            SetHeaderIfExists(dgvChiTietPhieu, "TenMon", "Món");
            SetHeaderIfExists(dgvChiTietPhieu, "SoLuong", "Số lượng");
            SetHeaderIfExists(dgvChiTietPhieu, "DonGia", "Đơn giá");
            SetHeaderIfExists(dgvChiTietPhieu, "ThanhTien", "Thành tiền");

            SetFormatIfExists(dgvChiTietPhieu, "SoLuong", "N0", DataGridViewContentAlignment.MiddleCenter);
            SetFormatIfExists(dgvChiTietPhieu, "DonGia", "N0", DataGridViewContentAlignment.MiddleRight);
            SetFormatIfExists(dgvChiTietPhieu, "ThanhTien", "N0", DataGridViewContentAlignment.MiddleRight);
        }

        private void frmNhapKhoUI_Load(object sender, EventArgs e)
        {
            try
            {
                LoadMonAn();
                LoadDanhSachPhieu();
                ClearChiTietNhap();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được dữ liệu nhập kho: " + ex.Message);
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
            dgvDanhSachPhieu.DataSource = nhapKhoBLL.LayDanhSachPhieuNhap();
        }

        private void LoadChiTietPhieu(int maPN)
        {
            dgvChiTietPhieu.DataSource = nhapKhoBLL.LayChiTietPhieuNhap(maPN);
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

            if (!decimal.TryParse(txtDonGia.Text.Trim(), out decimal donGia) || donGia <= 0)
            {
                MessageBox.Show("Đơn giá không hợp lệ");
                return;
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
                chiTietList.Add(new ChiTietPhieuNhapKho
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
                int maPN = nhapKhoBLL.TaoPhieuNhap(txtGhiChu.Text.Trim(), chiTietList.ToList());
                MessageBox.Show($"Đã lưu phiếu nhập: {maPN}");

                LoadDanhSachPhieu();
                ClearChiTietNhap();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu phiếu nhập: " + ex.Message);
            }
        }

        private void dgvDanhSachPhieu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvDanhSachPhieu.Rows[e.RowIndex];
            if (row.Cells["MaPN"].Value == null) return;

            selectedMaPN = Convert.ToInt32(row.Cells["MaPN"].Value);
            LoadChiTietPhieu(selectedMaPN.Value);
        }

        private void ClearChiTietNhap()
        {
            chiTietList.Clear();
            chiTietSource.ResetBindings(false);
            txtGhiChu.Clear();
        }
    }
}
