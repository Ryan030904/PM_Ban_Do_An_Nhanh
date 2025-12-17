using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmAddCustomer : Form
    {
        private KhachHangBLL khachHangBLL = new KhachHangBLL();

        public frmAddCustomer(string sdt = "")
        {
            InitializeComponent();
            this.Text = "Thêm khách hàng mới";
            txtSDT.Text = sdt;
            dtpNgaySinh.Checked = false;
            LoadKhachHangToGridView();
            SetupButtonStyles();
        }

        private void SetupButtonStyles()
        {
            // Style buttons với icons và tooltips
            ButtonStyleHelper.ApplySuccessStyle(btnLuu, "💾 Lưu", "Lưu thông tin khách hàng", ButtonSize.Medium);
            ButtonStyleHelper.ApplyWarningStyle(btnSua, "✏️ Sửa", "Chỉnh sửa khách hàng đã chọn", ButtonSize.Medium);
            ButtonStyleHelper.ApplyDangerStyle(btnHuy, "❌ Hủy", "Hủy và đóng form", ButtonSize.Medium);
            ButtonStyleHelper.ApplyDangerStyle(btnXoa, "🗑️ Xóa", "Xóa khách hàng đã chọn", ButtonSize.Medium);
            
            // Style các controls khác
            ButtonStyleHelper.ApplyModernTextBoxStyle(txtTenKH);
            ButtonStyleHelper.ApplyModernTextBoxStyle(txtSDT);
            ButtonStyleHelper.ApplyModernTextBoxStyle(txtDiaChi);
            ButtonStyleHelper.ApplyModernTextBoxStyle(txtEmail);
            
            // Style DataGridView
            ButtonStyleHelper.ApplyModernDataGridViewStyle(dgvKhachHang, "primary");
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string tenKH = txtTenKH.Text.Trim();
            string sdt = txtSDT.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();
            string email = txtEmail.Text.Trim();
            DateTime? ngaySinh = dtpNgaySinh.Checked ? (DateTime?)dtpNgaySinh.Value : (DateTime?)null;

            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdt))
            {
                MessageBox.Show("Vui lòng nhập Tên khách hàng và Số điện thoại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                bool success = false;

                if (btnLuu.Text == "Cập nhật")
                {
                    success = khachHangBLL.CapNhatKhachHang(tenKH, sdt, diaChi, email, ngaySinh);
                    if (success)
                    {
                        MessageBox.Show("Cập nhật thông tin khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                        LoadKhachHangToGridView();
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật thông tin khách hàng thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    if (khachHangBLL.LayThongTinKhachHangBySDT(sdt) != null)
                    {
                        MessageBox.Show("Số điện thoại này đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    success = khachHangBLL.ThemKhachHang(tenKH, sdt, diaChi, email, ngaySinh);
                    if (success)
                    {
                        MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Thêm khách hàng thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            txtTenKH.Text = "";
            txtSDT.Text = "";
            txtDiaChi.Text = "";
            txtEmail.Text = "";
            dtpNgaySinh.Checked = false;
            btnLuu.Text = "Lưu";
            txtSDT.Enabled = true;
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoadKhachHangToGridView()
        {
            try
            {
                dgvKhachHang.DataSource = khachHangBLL.HienThiDanhSachKhachHang();
                
                // Thiết lập độ rộng cột cân bằng và hợp lý
                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                
                // Mã KH - compact hơn
                if (dgvKhachHang.Columns.Contains("MaKH"))
                {
                    dgvKhachHang.Columns["MaKH"].HeaderText = "Mã KH";
                    dgvKhachHang.Columns["MaKH"].Width = 80;
                    dgvKhachHang.Columns["MaKH"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                
                // Tên khách hàng - rộng hơn để hiển thị đầy đủ
                if (dgvKhachHang.Columns.Contains("TenKH"))
                {
                    dgvKhachHang.Columns["TenKH"].HeaderText = "Tên khách hàng";
                    dgvKhachHang.Columns["TenKH"].Width = 150;
                }
                
                // SĐT - rộng hơn cho số điện thoại dài
                if (dgvKhachHang.Columns.Contains("SDT"))
                {
                    dgvKhachHang.Columns["SDT"].HeaderText = "Số điện thoại";
                    dgvKhachHang.Columns["SDT"].Width = 110;
                    dgvKhachHang.Columns["SDT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                
                // Địa chỉ - vừa phải
                if (dgvKhachHang.Columns.Contains("DiaChi"))
                {
                    dgvKhachHang.Columns["DiaChi"].HeaderText = "Địa chỉ";
                    dgvKhachHang.Columns["DiaChi"].Width = 200;
                }
                
                // Email - vừa phải  
                if (dgvKhachHang.Columns.Contains("Email"))
                {
                    dgvKhachHang.Columns["Email"].HeaderText = "Email";
                    dgvKhachHang.Columns["Email"].Width = 140;
                }
                
                // Ngày sinh - rộng hơn để hiển thị đầy đủ
                if (dgvKhachHang.Columns.Contains("NgaySinh"))
                {
                    dgvKhachHang.Columns["NgaySinh"].HeaderText = "Ngày sinh";
                    dgvKhachHang.Columns["NgaySinh"].Width = 110;
                    dgvKhachHang.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    dgvKhachHang.Columns["NgaySinh"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvKhachHang.CurrentRow != null)
            {
                try
                {
                    string sdt = dgvKhachHang.CurrentRow.Cells["SDT"].Value.ToString();

                    var khachHang = khachHangBLL.LayThongTinKhachHangBySDT(sdt);

                    if (khachHang != null)
                    {
                        txtTenKH.Text = khachHang.TenKH;
                        txtSDT.Text = khachHang.SDT;
                        txtDiaChi.Text = khachHang.DiaChi ?? "";
                        txtEmail.Text = khachHang.Email ?? "";

                        if (khachHang.NgaySinh.HasValue)
                        {
                            dtpNgaySinh.Checked = true;
                            dtpNgaySinh.Value = khachHang.NgaySinh.Value;
                        }
                        else
                        {
                            dtpNgaySinh.Checked = false;
                        }
                        btnLuu.Text = "Cập nhật";
                        txtSDT.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thông tin khách hàng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lấy thông tin khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một khách hàng từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                btnSua_Click(sender, EventArgs.Empty);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvKhachHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một khách hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sdt = dgvKhachHang.CurrentRow.Cells["SDT"].Value.ToString();
            string tenKH = dgvKhachHang.CurrentRow.Cells["TenKH"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng {tenKH} (SĐT: {sdt})?", "Xác nhận xóa",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = khachHangBLL.XoaKhachHang(sdt);

                    if (success)
                    {
                        MessageBox.Show("Xóa khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadKhachHangToGridView();
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Xóa khách hàng thất bại. Khách hàng này có thể đang được sử dụng trong đơn hàng.",
                                       "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

