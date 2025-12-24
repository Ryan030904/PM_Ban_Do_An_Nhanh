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
    public class frmXuatKho : Form
    {
        private readonly MonAnBLL monAnBLL = new MonAnBLL();
        private readonly XuatKhoBLL xuatKhoBLL = new XuatKhoBLL();

        private readonly ComboBox cboMon = new ComboBox();
        private readonly TextBox txtSoLuong = new TextBox();
        private readonly TextBox txtDonGia = new TextBox();
        private readonly TextBox txtLyDo = new TextBox();
        private readonly TextBox txtGhiChu = new TextBox();
        private readonly Button btnThemDong = new Button();
        private readonly Button btnLuuPhieu = new Button();

        private readonly DataGridView dgvChiTietXuat = new DataGridView();
        private readonly DataGridView dgvDanhSachPhieu = new DataGridView();
        private readonly DataGridView dgvChiTietPhieu = new DataGridView();

        private readonly BindingSource chiTietSource = new BindingSource();
        private readonly List<ChiTietPhieuXuatKho> chiTietList = new List<ChiTietPhieuXuatKho>();

        private int? selectedMaPX;

        public frmXuatKho()
        {
            Text = "Xuất kho";
            Font = new Font("Segoe UI", 10F);
            Width = 1100;
            Height = 680;
            StartPosition = FormStartPosition.CenterScreen;

            InitializeLayout();
            Load += frmXuatKho_Load;
        }

        private void InitializeLayout()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 320
            };
            Controls.Add(split);

            var grpCreate = new GroupBox { Text = "Tạo phiếu xuất", Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(grpCreate);

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 120 };
            grpCreate.Controls.Add(pnlTop);

            var lblMon = new Label { Text = "Món:", AutoSize = true, Location = new Point(12, 15) };
            cboMon.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMon.Location = new Point(90, 12);
            cboMon.Width = 320;

            var lblSoLuong = new Label { Text = "Số lượng:", AutoSize = true, Location = new Point(430, 15) };
            txtSoLuong.Location = new Point(510, 12);
            txtSoLuong.Width = 90;

            var lblDonGia = new Label { Text = "Đơn giá (tuỳ chọn):", AutoSize = true, Location = new Point(620, 15) };
            txtDonGia.Location = new Point(760, 12);
            txtDonGia.Width = 120;

            btnThemDong.Text = "Thêm";
            btnThemDong.Location = new Point(900, 10);
            btnThemDong.Width = 90;
            btnThemDong.Click += btnThemDong_Click;

            btnLuuPhieu.Text = "Lưu phiếu";
            btnLuuPhieu.Location = new Point(990, 10);
            btnLuuPhieu.Width = 90;
            btnLuuPhieu.Click += btnLuuPhieu_Click;

            var lblLyDo = new Label { Text = "Lý do:", AutoSize = true, Location = new Point(12, 55) };
            txtLyDo.Location = new Point(90, 52);
            txtLyDo.Width = 790;

            var lblGhiChu = new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(12, 90) };
            txtGhiChu.Location = new Point(90, 87);
            txtGhiChu.Width = 790;

            pnlTop.Controls.Add(lblMon);
            pnlTop.Controls.Add(cboMon);
            pnlTop.Controls.Add(lblSoLuong);
            pnlTop.Controls.Add(txtSoLuong);
            pnlTop.Controls.Add(lblDonGia);
            pnlTop.Controls.Add(txtDonGia);
            pnlTop.Controls.Add(btnThemDong);
            pnlTop.Controls.Add(btnLuuPhieu);
            pnlTop.Controls.Add(lblLyDo);
            pnlTop.Controls.Add(txtLyDo);
            pnlTop.Controls.Add(lblGhiChu);
            pnlTop.Controls.Add(txtGhiChu);

            dgvChiTietXuat.Dock = DockStyle.Fill;
            dgvChiTietXuat.AllowUserToAddRows = false;
            dgvChiTietXuat.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChiTietXuat.ReadOnly = true;
            grpCreate.Controls.Add(dgvChiTietXuat);

            var grpList = new GroupBox { Text = "Danh sách phiếu xuất", Dock = DockStyle.Fill };
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
            dgvChiTietXuat.DataSource = chiTietSource;
        }

        private void frmXuatKho_Load(object sender, EventArgs e)
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
    }
}
