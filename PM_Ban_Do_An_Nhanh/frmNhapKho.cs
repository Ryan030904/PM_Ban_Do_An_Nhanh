using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh
{
    public class frmNhapKho : Form
    {
        private readonly MonAnBLL monAnBLL = new MonAnBLL();
        private readonly NhapKhoBLL nhapKhoBLL = new NhapKhoBLL();

        private readonly ComboBox cboMon = new ComboBox();
        private readonly TextBox txtSoLuong = new TextBox();
        private readonly TextBox txtDonGia = new TextBox();
        private readonly TextBox txtGhiChu = new TextBox();
        private readonly Button btnThemDong = new Button();
        private readonly Button btnLuuPhieu = new Button();

        private readonly DataGridView dgvChiTietNhap = new DataGridView();
        private readonly DataGridView dgvDanhSachPhieu = new DataGridView();
        private readonly DataGridView dgvChiTietPhieu = new DataGridView();

        private readonly BindingSource chiTietSource = new BindingSource();
        private readonly List<ChiTietPhieuNhapKho> chiTietList = new List<ChiTietPhieuNhapKho>();

        private int? selectedMaPN;

        public frmNhapKho()
        {
            Text = "Nhập kho";
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            InitializeLayout();
            Load += frmNhapKho_Load;
        }

        private void InitializeLayout()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 290
            };
            Controls.Add(split);

            var grpCreate = new GroupBox { Text = "Tạo phiếu nhập", Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(grpCreate);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90 };
            grpCreate.Controls.Add(pnlTop);

            var lblMon = new Label { Text = "Món:", AutoSize = true, Location = new Point(12, 15) };
            cboMon.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMon.Location = new Point(90, 12);
            cboMon.Width = 320;

            var lblSoLuong = new Label { Text = "Số lượng:", AutoSize = true, Location = new Point(430, 15) };
            txtSoLuong.Location = new Point(510, 12);
            txtSoLuong.Width = 90;

            var lblDonGia = new Label { Text = "Đơn giá:", AutoSize = true, Location = new Point(620, 15) };
            txtDonGia.Location = new Point(690, 12);
            txtDonGia.Width = 120;

            btnThemDong.Text = "Thêm";
            btnThemDong.Location = new Point(830, 10);
            btnThemDong.Width = 90;
            btnThemDong.Click += btnThemDong_Click;

            btnLuuPhieu.Text = "Lưu phiếu";
            btnLuuPhieu.Location = new Point(930, 10);
            btnLuuPhieu.Width = 120;
            btnLuuPhieu.Click += btnLuuPhieu_Click;

            var lblGhiChu = new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(12, 55) };
            txtGhiChu.Location = new Point(90, 52);
            txtGhiChu.Width = 720;

            pnlTop.Controls.Add(lblMon);
            pnlTop.Controls.Add(cboMon);
            pnlTop.Controls.Add(lblSoLuong);
            pnlTop.Controls.Add(txtSoLuong);
            pnlTop.Controls.Add(lblDonGia);
            pnlTop.Controls.Add(txtDonGia);
            pnlTop.Controls.Add(btnThemDong);
            pnlTop.Controls.Add(btnLuuPhieu);
            pnlTop.Controls.Add(lblGhiChu);
            pnlTop.Controls.Add(txtGhiChu);

            dgvChiTietNhap.Dock = DockStyle.Fill;
            dgvChiTietNhap.AllowUserToAddRows = false;
            dgvChiTietNhap.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChiTietNhap.ReadOnly = true;
            grpCreate.Controls.Add(dgvChiTietNhap);

            var grpList = new GroupBox { Text = "Danh sách phiếu nhập", Dock = DockStyle.Fill };
            split.Panel2.Controls.Add(grpList);

            var split2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 520
            };
            grpList.Controls.Add(split2);

            dgvDanhSachPhieu.Dock = DockStyle.Fill;
            dgvDanhSachPhieu.ReadOnly = true;
            dgvDanhSachPhieu.AllowUserToAddRows = false;
            dgvDanhSachPhieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDanhSachPhieu.CellClick += dgvDanhSachPhieu_CellClick;
            split2.Panel1.Controls.Add(dgvDanhSachPhieu);

            dgvChiTietPhieu.Dock = DockStyle.Fill;
            dgvChiTietPhieu.ReadOnly = true;
            dgvChiTietPhieu.AllowUserToAddRows = false;
            dgvChiTietPhieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            split2.Panel2.Controls.Add(dgvChiTietPhieu);

            chiTietSource.DataSource = chiTietList;
            dgvChiTietNhap.DataSource = chiTietSource;
        }

        private void frmNhapKho_Load(object sender, EventArgs e)
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
