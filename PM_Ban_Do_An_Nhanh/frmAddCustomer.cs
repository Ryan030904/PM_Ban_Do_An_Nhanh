using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.UI;
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

        private readonly bool isLookupOnly;
        private readonly bool enableSearch;
        private DataTable allCustomersDt;
        private bool suppressFilter;

        private Panel rightHost;
        private FlowLayoutPanel pnlStatusFilter;
        private ComboBox cboStatusFilter;
        private bool statusFilterInitialized;
        private string currentStatusMode = "Active";

        private string editingSdtOld;

        private TextBox txtLookupTen;
        private TextBox txtLookupSDT;
        private FlowLayoutPanel pnlLookupTop;

        public frmAddCustomer(string sdt = "", bool lookupOnly = false, bool enableSearch = false)
        {
            InitializeComponent();
            isLookupOnly = lookupOnly;
            this.enableSearch = enableSearch;
            this.Text = isLookupOnly ? "Tra cứu khách hàng" : "Thêm khách hàng mới";
            txtSDT.Text = sdt;

            try
            {
                txtTenKH.Leave += (s, e) =>
                {
                    try
                    {
                        string raw = txtTenKH.Text;
                        string normalized = khachHangBLL.ChuanHoaTenKhachHang(raw);
                        if (!string.Equals(raw ?? "", normalized ?? "", StringComparison.Ordinal))
                            txtTenKH.Text = normalized;
                    }
                    catch { }
                };
            }
            catch { }

            EnsureStatusFilterUI();
            LoadKhachHangToGridView();
            SetupButtonStyles();

            if (isLookupOnly)
                ApplyLookupOnlyMode();
            else if (this.enableSearch)
                ApplySearchMode();
        }

        private void EnsureStatusFilterUI()
        {
            if (statusFilterInitialized) return;

            try
            {
                cboStatusFilter = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 180
                };
                cboStatusFilter.Items.AddRange(new object[] { "Đang hoạt động", "Đã ẩn", "Tất cả" });
                cboStatusFilter.SelectedIndex = 0;
                currentStatusMode = "Active";
                cboStatusFilter.SelectedIndexChanged += (s, e) =>
                {
                    currentStatusMode = GetStatusMode();
                    LoadKhachHangToGridView();
                };

                if (dgvKhachHang != null && dgvKhachHang.Parent == this)
                {
                    Controls.Remove(dgvKhachHang);

                    rightHost = new Panel { Dock = DockStyle.Fill };
                    Controls.Add(rightHost);
                    rightHost.BringToFront();

                    pnlStatusFilter = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 46,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false,
                        Padding = new Padding(12, 10, 12, 6)
                    };

                    var lblMode = new Label { Text = "Hiển thị:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) };
                    pnlStatusFilter.Controls.Add(lblMode);
                    pnlStatusFilter.Controls.Add(cboStatusFilter);

                    dgvKhachHang.Dock = DockStyle.Fill;
                    rightHost.Controls.Add(dgvKhachHang);
                    rightHost.Controls.Add(pnlStatusFilter);
                }

                statusFilterInitialized = true;
            }
            catch
            {
                statusFilterInitialized = true;
            }
        }

        private string GetStatusMode()
        {
            try
            {
                if (cboStatusFilter == null) return currentStatusMode;
                string t = cboStatusFilter.SelectedItem?.ToString() ?? "";
                if (string.Equals(t, "Đang hoạt động", StringComparison.OrdinalIgnoreCase)) return "Active";
                if (string.Equals(t, "Đã ẩn", StringComparison.OrdinalIgnoreCase)) return "Inactive";
                return "All";
            }
            catch
            {
                return currentStatusMode;
            }
        }

        private void ApplySearchMode()
        {
            EnsureManagementLayout();

            txtTenKH.TextChanged -= Filter_TextChanged;
            txtTenKH.TextChanged += Filter_TextChanged;
            txtSDT.TextChanged -= Filter_TextChanged;
            txtSDT.TextChanged += Filter_TextChanged;

            ApplyFilter();
        }

        private void EnsureManagementLayout()
        {
            try
            {
                if (panel1 != null)
                {
                    panel1.Visible = true;
                    panel1.AutoScroll = true;
                    panel1.Width = 380;
                    panel1.MinimumSize = new Size(360, 0);
                }

                if (dgvKhachHang != null)
                {
                    dgvKhachHang.Dock = DockStyle.Fill;
                }

                int left = 20;
                int width = (panel1 != null ? panel1.Width : 380) - (left * 2);
                width = Math.Max(200, width);

                if (txtTenKH != null)
                {
                    txtTenKH.Left = left;
                    txtTenKH.Width = width;
                    txtTenKH.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }

                if (txtSDT != null)
                {
                    txtSDT.Left = left;
                    txtSDT.Width = width;
                    txtSDT.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }


                if (btnLuu != null)
                {
                    btnLuu.Left = left;
                    btnLuu.Width = width;
                    btnLuu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
                if (btnSua != null)
                {
                    btnSua.Left = left;
                    btnSua.Width = width;
                    btnSua.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
                if (btnXoa != null)
                {
                    btnXoa.Left = left;
                    btnXoa.Width = width;
                    btnXoa.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
                if (btnHuy != null)
                {
                    btnHuy.Left = left;
                    btnHuy.Width = width;
                    btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }

                if (label1 != null) label1.Left = left;
                if (label2 != null) label2.Left = left;
            }
            catch
            {
            }
        }

        private void ApplyLookupOnlyMode()
        {
            // This screen is used for lookup/search only.
            // Keep the grid visible and allow typing in filters.

            btnLuu.Visible = false;
            btnSua.Visible = false;
            btnXoa.Visible = false;
            btnHuy.Visible = false;

            try
            {
                // Hide the left edit panel and replace it with a compact top search bar
                panel1.Visible = false;

                if (pnlLookupTop == null)
                {
                    pnlLookupTop = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        Height = 54,
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false,
                        Padding = new Padding(12, 10, 12, 10)
                    };

                    var lblTen = new Label { Text = "Tên:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) };
                    txtLookupTen = new TextBox { Width = 220 };
                    var lblSdt = new Label { Text = "SĐT:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 6, 0, 0) };
                    txtLookupSDT = new TextBox { Width = 180 };

                    pnlLookupTop.Controls.Add(lblTen);
                    pnlLookupTop.Controls.Add(txtLookupTen);
                    pnlLookupTop.Controls.Add(lblSdt);
                    pnlLookupTop.Controls.Add(txtLookupSDT);

                    if (cboStatusFilter != null)
                    {
                        var lblMode = new Label { Text = "Hiển thị:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 6, 0, 0) };
                        pnlLookupTop.Controls.Add(lblMode);
                        pnlLookupTop.Controls.Add(cboStatusFilter);
                    }

                    txtLookupTen.TextChanged += Filter_TextChanged;
                    txtLookupSDT.TextChanged += Filter_TextChanged;

                    Controls.Add(pnlLookupTop);
                    pnlLookupTop.BringToFront();
                }

                dgvKhachHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch
            {
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (!(isLookupOnly || enableSearch)) return;
            if (allCustomersDt == null) return;
            if (suppressFilter) return;

            try
            {
                string ten = ((txtLookupTen != null ? txtLookupTen.Text : txtTenKH.Text) ?? "").Trim().Replace("'", "''");
                string sdt = ((txtLookupSDT != null ? txtLookupSDT.Text : txtSDT.Text) ?? "").Trim().Replace("'", "''");

                var dv = allCustomersDt.DefaultView;
                string filter = "";

                if (!string.IsNullOrEmpty(ten))
                    filter += $"TenKH LIKE '%{ten}%'";

                if (!string.IsNullOrEmpty(sdt))
                {
                    if (!string.IsNullOrEmpty(filter)) filter += " AND ";
                    filter += $"SDT LIKE '%{sdt}%'";
                }

                dv.RowFilter = filter;
                dgvKhachHang.DataSource = dv;
            }
            catch
            {
            }
        }

        private void SetupButtonStyles()
        {
            // Style buttons với icons và tooltips
            btnLuu.Text = "Lưu";
            btnSua.Text = "Sửa";
            btnHuy.Text = "Hủy";
            btnXoa.Text = "Xóa";
            
            // Style các controls khác
            txtTenKH.BorderStyle = BorderStyle.Fixed3D;
            txtSDT.BorderStyle = BorderStyle.Fixed3D;
            
            // Style DataGridView
            dgvKhachHang.EnableHeadersVisualStyles = true;
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string tenKH = txtTenKH.Text.Trim();
            string sdt = txtSDT.Text.Trim();
            string diaChi = "";

            try
            {
                tenKH = khachHangBLL.ChuanHoaTenKhachHang(tenKH);
                if (!string.Equals(txtTenKH.Text ?? "", tenKH ?? "", StringComparison.Ordinal))
                    txtTenKH.Text = tenKH;
            }
            catch { }

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
                    string sdtCu = (editingSdtOld ?? "").Trim();

                    if (!string.IsNullOrWhiteSpace(sdtCu) && !string.Equals(sdtCu, sdt, StringComparison.OrdinalIgnoreCase))
                    {
                        var existed = khachHangBLL.LayThongTinKhachHangBySDT(sdt);
                        if (existed != null)
                        {
                            MessageBox.Show("Số điện thoại này đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(sdtCu))
                        success = khachHangBLL.CapNhatKhachHangTheoSdtCu(tenKH, sdt, diaChi, sdtCu);
                    else
                        success = khachHangBLL.CapNhatKhachHang(tenKH, sdt, diaChi);
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
                    success = khachHangBLL.ThemKhachHang(tenKH, sdt, diaChi);
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
            btnLuu.Text = "Lưu";
            txtSDT.Enabled = true;
            editingSdtOld = null;
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
                if (dgvKhachHang == null) return;

                // Use designer-defined columns (MaKH/TenKH/SDT/DiaChi)
                dgvKhachHang.AutoGenerateColumns = false;

                string mode = GetStatusMode();
                var dt = khachHangBLL?.HienThiDanhSachKhachHangTomTatTheoTrangThai(mode);
                allCustomersDt = dt ?? new DataTable();

                try
                {
                    if (!allCustomersDt.Columns.Contains("Rank"))
                        allCustomersDt.Columns.Add("Rank", typeof(string));

                    if (allCustomersDt.Columns.Contains("TongChiTieu"))
                    {
                        foreach (DataRow r in allCustomersDt.Rows)
                        {
                            decimal tong = 0m;
                            try
                            {
                                if (r["TongChiTieu"] != DBNull.Value)
                                    decimal.TryParse(r["TongChiTieu"].ToString(), out tong);
                            }
                            catch { tong = 0m; }

                            r["Rank"] = khachHangBLL.TinhRank(tong);
                        }
                    }
                }
                catch
                {
                }

                dgvKhachHang.DataSource = allCustomersDt;

                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvKhachHang.AllowUserToAddRows = false;
                dgvKhachHang.AllowUserToDeleteRows = false;
                dgvKhachHang.ReadOnly = true;
                dgvKhachHang.MultiSelect = false;
                dgvKhachHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                TableStyleHelper.DisableSorting(dgvKhachHang);

                var colMa = dgvKhachHang.Columns["MaKH"];
                if (colMa != null)
                {
                    colMa.HeaderText = "Mã KH";
                    colMa.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                var colTen = dgvKhachHang.Columns["TenKH"];
                if (colTen != null)
                {
                    colTen.HeaderText = "Tên khách hàng";
                }

                var colSdt = dgvKhachHang.Columns["SDT"];
                if (colSdt != null)
                {
                    colSdt.HeaderText = "Số điện thoại";
                    colSdt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                var colRank = dgvKhachHang.Columns["Rank"];
                if (colRank != null)
                {
                    colRank.HeaderText = "Hạng";
                    colRank.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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
                        suppressFilter = true;
                        txtTenKH.Text = khachHang.TenKH;
                        txtSDT.Text = khachHang.SDT;
                        btnLuu.Text = "Cập nhật";
                        txtSDT.Enabled = true;
                        editingSdtOld = khachHang.SDT;
                        try
                        {
                            bool isInactive = string.Equals(khachHang.TrangThai ?? "", "Inactive", StringComparison.OrdinalIgnoreCase);
                            btnXoa.Text = isInactive ? "Kích hoạt lại" : "Ngưng hoạt động";
                        }
                        catch { }
                        suppressFilter = false;
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
                if (isLookupOnly) return;
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

            string currentTrangThai = "Active";
            try
            {
                var kh = khachHangBLL.LayThongTinKhachHangBySDT(sdt);
                if (kh != null && !string.IsNullOrWhiteSpace(kh.TrangThai))
                    currentTrangThai = kh.TrangThai;
            }
            catch { }

            bool isInactive = string.Equals(currentTrangThai, "Inactive", StringComparison.OrdinalIgnoreCase);
            string target = isInactive ? "Active" : "Inactive";
            string actionText = isInactive ? "kích hoạt lại" : "ngưng hoạt động";

            if (MessageBox.Show($"Bạn có chắc chắn muốn {actionText} khách hàng {tenKH} (SĐT: {sdt})?", "Xác nhận",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    bool success = khachHangBLL.CapNhatTrangThaiKhachHang(sdt, target);

                    if (success)
                    {
                        MessageBox.Show("Cập nhật trạng thái khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadKhachHangToGridView();
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật trạng thái. Vui lòng kiểm tra cột TrangThai trong bảng KhachHang.",
                                       "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật trạng thái khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

