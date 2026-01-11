using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Controls;
using PM_Ban_Do_An_Nhanh.Helpers;
using PM_Ban_Do_An_Nhanh.UI;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Collections.Generic;
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

        // Added controls
        private CartControl cartControl;
        private ContextMenuStrip dgvContextMenu;

        public frmMenuManagement()
        {
            InitializeComponent();

            this.Text = "Quản lý thực đơn";

            // GẮN EVENT
            dgvMonAn.CellClick += dgvMonAn_CellClick;
            btnThem.Click += btnThem_Click;
            btnSua.Click += btnSua_Click;
            btnXoa.Click += btnXoa_Click;
            btnLamMoi.Click += btnLamMoi_Click;
            btnChonHinh.Click += btnChonHinh_Click;

            txtGia.Enter += (s, e) => UnformatGiaTextBox();
            txtGia.Leave += (s, e) => FormatGiaTextBox();

            // Ensure we handle column visibility after binding completes
            dgvMonAn.DataBindingComplete += DgvMonAn_DataBindingComplete;

            // Add context menu on the grid for quick actions (Edit price)
            dgvContextMenu = new ContextMenuStrip();
            dgvContextMenu.Items.Add("Sửa giá", null, DgvContext_EditPrice_Click);
            dgvContextMenu.Items.Add("Nhập hàng", null, DgvContext_ImportStock_Click);
            dgvMonAn.ContextMenuStrip = dgvContextMenu;

            // Add cart control (dock to right) so item cards can add to cart
            cartControl = new CartControl();
            cartControl.PlaceOrderClicked += CartControl_PlaceOrderClicked;
            if (panelCartHost != null)
            {
                cartControl.Dock = DockStyle.Fill;
                panelCartHost.Controls.Add(cartControl);
            }

            SetupTrangThaiComboBox();
            LoadDanhMucToComboBox();
            LoadDataToDataGridView();
            ClearInputFields();
            SetupButtonStyles();
        }

        private static decimal ParseVndToDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0m;

            string s = input.Trim();
            // .NET Framework 4.8: string.Replace has no StringComparison overload
            s = s.Replace("VNĐ", "")
                 .Replace("vnđ", "")
                 .Replace("VND", "")
                 .Replace("vnd", "")
                 .Replace("₫", "")
                 .Replace("đ", "")
                 .Replace("Đ", "")
                 .Trim();

            // Remove common thousand separators/spaces
            s = s.Replace(" ", "").Replace(".", "").Replace(",", "");

            if (decimal.TryParse(s, out decimal v)) return v;
            return 0m;
        }

        private static string FormatDecimalToVnd(decimal value)
        {
            return string.Format("{0:N0} VNĐ", value);
        }

        private void FormatGiaTextBox()
        {
            try
            {
                decimal v = ParseVndToDecimal(txtGia.Text);
                if (v > 0) txtGia.Text = FormatDecimalToVnd(v);
            }
            catch { }
        }

        private void UnformatGiaTextBox()
        {
            try
            {
                decimal v = ParseVndToDecimal(txtGia.Text);
                if (v > 0) txtGia.Text = v.ToString("0");
            }
            catch { }
        }

        // ======================= STYLE =======================
        private void SetupButtonStyles()
        {
            btnThem.Text = "Thêm";
            btnSua.Text = "Sửa";
            btnXoa.Text = "Xóa";
            btnLamMoi.Text = "Làm mới";
            btnChonHinh.Text = "Chọn hình";

            picHinhAnh.SizeMode = PictureBoxSizeMode.Zoom;
            picHinhAnh.BorderStyle = BorderStyle.FixedSingle;
        }

        // ======================= LOAD DATA =======================
        private void LoadDataToDataGridView()
        {
            try
            {
                if (dgvMonAn == null) return;

                // Get DataTable from BLL; ensure non-null
                DataTable dt = monAnBLL.HienThiDanhSachMonAn() ?? new DataTable();

                // Ensure columns will be auto-generated from the DataTable
                dgvMonAn.AutoGenerateColumns = true;

                // Set the DataSource (columns are generated asynchronously by the binding system)
                dgvMonAn.DataSource = dt;
                dgvMonAn.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // VNĐ formatting for price column if present
                TableStyleHelper.ApplyVndFormatting(dgvMonAn, "Gia");

                // Do NOT access dgvMonAn.Columns immediately here — wait for DataBindingComplete.
            }
            catch (Exception ex)
            {
                // Log so you can inspect unexpected failures during UI setup
                Logger.Log(ex);
            }
        }

        // Fires after the grid has finished creating columns for the DataSource
        private void DgvMonAn_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                var dgv = sender as DataGridView;
                if (dgv == null) return;

                // Only proceed when DataSource is a DataTable (our expected case)
                if (dgv.DataSource is DataTable dt)
                {
                    // Hide technical columns if present
                    if (dt.Columns.Contains("MaDM") && dgv.Columns.Contains("MaDM"))
                        dgv.Columns["MaDM"].Visible = false;
                    if (dt.Columns.Contains("HinhAnh") && dgv.Columns.Contains("HinhAnh"))
                        dgv.Columns["HinhAnh"].Visible = false;
                }
                else
                {
                    // If DataSource is not a DataTable, perform a defensive check on columns
                    if (dgv.Columns.Contains("MaDM"))
                        dgv.Columns["MaDM"].Visible = false;
                    if (dgv.Columns.Contains("HinhAnh"))
                        dgv.Columns["HinhAnh"].Visible = false;
                }

                if (dgv.Columns.Contains("SoLuongTon"))
                {
                    dgv.Columns["SoLuongTon"].HeaderText = "Tồn kho";
                    dgv.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void DgvContext_ImportStock_Click(object sender, EventArgs e)
        {
            try
            {
                var row = GetSelectedGridRow();
                if (row == null)
                {
                    MessageBox.Show("Chưa chọn món để nhập hàng.");
                    return;
                }

                if (row.Cells["MaMon"].Value == DBNull.Value) return;

                int maMon = Convert.ToInt32(row.Cells["MaMon"].Value);
                string tenMon = row.Cells["TenMon"].Value?.ToString() ?? "";

                int soLuongNhap = PromptQuantity($"Nhập hàng - {tenMon}");
                if (soLuongNhap <= 0) return;

                bool ok = monAnBLL.NhapHang(maMon, soLuongNhap);
                if (ok)
                {
                    LoadDataToDataGridView();
                    MessageBox.Show("Nhập hàng thành công.");
                }
                else
                {
                    MessageBox.Show("Nhập hàng thất bại.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Lỗi nhập hàng: " + ex.Message);
            }
        }

        private int PromptQuantity(string title)
        {
            using (var f = new Form())
            {
                f.Text = title;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MinimizeBox = false;
                f.MaximizeBox = false;
                f.Width = 320;
                f.Height = 160;

                var lbl = new Label { Left = 12, Top = 16, Width = 280, Text = "Số lượng nhập:" };
                var nud = new NumericUpDown { Left = 12, Top = 44, Width = 280, Minimum = 1, Maximum = 100000, Value = 1 };
                var btnOk = new Button { Text = "OK", Left = 132, Width = 76, Top = 80, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Hủy", Left = 216, Width = 76, Top = 80, DialogResult = DialogResult.Cancel };

                f.Controls.Add(lbl);
                f.Controls.Add(nud);
                f.Controls.Add(btnOk);
                f.Controls.Add(btnCancel);
                f.AcceptButton = btnOk;
                f.CancelButton = btnCancel;

                return f.ShowDialog(this) == DialogResult.OK ? (int)nud.Value : 0;
            }
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
            try
            {
                decimal v = 0m;
                if (row.Cells["Gia"].Value != null && row.Cells["Gia"].Value != DBNull.Value)
                    decimal.TryParse(row.Cells["Gia"].Value.ToString(), out v);
                txtGia.Text = v > 0 ? FormatDecimalToVnd(v) : "";
            }
            catch
            {
                txtGia.Text = row.Cells["Gia"].Value?.ToString() ?? "";
            }
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
                ParseVndToDecimal(txtGia.Text),
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
                ParseVndToDecimal(txtGia.Text),
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
            decimal g = ParseVndToDecimal(txtGia.Text);
            if (g <= 0) return false;
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

        // ======================= ITEM CARD CREATION =======================
        private void CreateItemCard(DataRow itemRow)
        {
            var pnl = new Panel { Width = 240, Height = 160, Margin = new Padding(8), BorderStyle = BorderStyle.FixedSingle };

            // bigger picture for nicer look
            var pb = new PictureBox
            {
                Size = new Size(160, 100), // increased size for better visuals
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 8,
                Top = 8,
                Image = ImageHelper.LoadMenuItemImage(itemRow["HinhAnh"]?.ToString() ?? "")
            };

            var lblName = new Label { Text = $"{itemRow["MaMon"]} {itemRow["TenMon"]}", Left = 8, Top = 112, Width = 160, AutoEllipsis = true };
            var lblPrice = new Label { Text = $"{Convert.ToDecimal(itemRow["Gia"]):N0} VNĐ", ForeColor = Color.DarkRed, Left = 8, Top = 132, Width = 100 };

            var nudQty = new NumericUpDown { Left = 170, Top = 128, Width = 44, Minimum = 1, Value = 1 };
            var btnAdd = new Button { Text = "Thêm", Left = 216, Top = 126, Width = 52, BackColor = Color.FromArgb(37, 150, 84), ForeColor = Color.White };

            int maMon = Convert.ToInt32(itemRow["MaMon"]);
            string tenMon = itemRow["TenMon"].ToString();
            decimal gia = Convert.ToDecimal(itemRow["Gia"]);

            btnAdd.Click += (s, e) =>
            {
                cartControl.AddItem(maMon, tenMon, gia, (int)nudQty.Value);
            };

            pnl.Controls.Add(pb);
            pnl.Controls.Add(lblName);
            pnl.Controls.Add(lblPrice);
            pnl.Controls.Add(nudQty);
            pnl.Controls.Add(btnAdd);
        }

        // ======================= CONTEXT MENU: EDIT PRICE =======================
        private void DgvContext_EditPrice_Click(object sender, EventArgs e)
        {
            try
            {
                var row = GetSelectedGridRow();
                if (row == null)
                {
                    MessageBox.Show("Chưa chọn món để chỉnh giá.");
                    return;
                }

                if (row.Cells["MaMon"].Value == DBNull.Value) return;

                int maMon = Convert.ToInt32(row.Cells["MaMon"].Value);
                decimal currentPrice = Convert.ToDecimal(row.Cells["Gia"].Value);

                using (var dlg = new frmEditPrice(currentPrice))
                {
                    var res = dlg.ShowDialog(this);
                    if (res == DialogResult.OK)
                    {
                        decimal newPrice = dlg.NewPrice;

                        // read other fields needed by SuaMonAn
                        string tenMon = row.Cells["TenMon"].Value?.ToString() ?? "";
                        int maDM = row.Cells["MaDM"].Value == DBNull.Value ? 0 : Convert.ToInt32(row.Cells["MaDM"].Value);
                        string trangThai = row.Cells["TrangThai"].Value?.ToString() ?? "Còn hàng";
                        string imgPath = row.Cells["HinhAnh"].Value == DBNull.Value ? null : row.Cells["HinhAnh"].Value.ToString();

                        bool ok = monAnBLL.SuaMonAn(maMon, tenMon, newPrice, maDM, trangThai, imgPath);
                        if (ok)
                        {
                            LoadDataToDataGridView();
                            MessageBox.Show("Đã cập nhật giá.");
                        }
                        else
                        {
                            MessageBox.Show("Cập nhật thất bại.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private DataGridViewRow GetSelectedGridRow()
        {
            if (dgvMonAn.CurrentRow != null && dgvMonAn.CurrentRow.Index >= 0)
                return dgvMonAn.CurrentRow;

            if (selectedMonAnId != -1)
            {
                foreach (DataGridViewRow r in dgvMonAn.Rows)
                {
                    if (r.Cells["MaMon"].Value != DBNull.Value && Convert.ToInt32(r.Cells["MaMon"].Value) == selectedMonAnId)
                        return r;
                }
            }
            return null;
        }

        // ======================= CART EVENTS =======================
        private void CartControl_PlaceOrderClicked(object sender, EventArgs e)
        {
            try
            {
                var dt = cartControl.GetCartTable();
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Giỏ hàng trống.");
                    return;
                }

                decimal total = 0m;
                foreach (DataRow r in dt.Rows) total += Convert.ToDecimal(r["Total"]);
                MessageBox.Show($"Đang tạo đơn với {dt.Rows.Count} món. Tổng tiền: {TableStyleHelper.FormatVnd(total)}");

                var copy = cartControl.GetCartTable();
                foreach (DataRow r in copy.Rows)
                {
                    cartControl.RemoveItem(Convert.ToInt32(r["MaMon"]));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
namespace PM_Ban_Do_An_Nhanh.Controls
{
    public class CartControl : UserControl
    {
        private DataGridView dgvCart;
        private Label lblTotal;
        private Button btnPlaceOrder;
        private Button btnClear;
        private Label lblBadge;

        private DataTable cartTable;

        public event EventHandler PlaceOrderClicked;

        public CartControl()
        {
            InitializeComponents();
            BuildCartTable();
            BindTable();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Right;
            this.Width = 360;
            this.Padding = new Padding(8);

            // Badge (top-right)
            lblBadge = new Label
            {
                AutoSize = false,
                Size = new Size(26, 26),
                BackColor = Color.Red,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = false
            };
            // place badge on top-right of control (absolute layout)
            lblBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBadge.Location = new Point(this.Width - lblBadge.Width - 12, 8);

            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            lblTotal = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 4, 8, 0)
            };

            FlowLayoutPanel fl = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnPlaceOrder = new Button
            {
                Text = "Đặt hàng",
                AutoSize = true,
                BackColor = Color.FromArgb(37, 150, 84),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPlaceOrder.Click += (s, e) => PlaceOrderClicked?.Invoke(this, EventArgs.Empty);

            btnClear = new Button
            {
                Text = "Xóa hết",
                AutoSize = true
            };
            btnClear.Click += (s, e) => Clear();

            fl.Controls.Add(btnPlaceOrder);
            fl.Controls.Add(btnClear);

            var bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72
            };

            bottom.Controls.Add(fl);
            bottom.Controls.Add(lblTotal);

            // We add controls in z-order: badge on top so bring to front after adding others
            this.Controls.Add(dgvCart);
            this.Controls.Add(bottom);
            this.Controls.Add(lblBadge);
            lblBadge.BringToFront();
            this.Resize += (s, e) =>
            {
                // keep badge in top-right
                lblBadge.Location = new Point(Math.Max(8, this.Width - lblBadge.Width - 12), 8);
            };
        }

        private void BuildCartTable()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("MaMon", typeof(int));
            cartTable.Columns.Add("TenMon", typeof(string));
            cartTable.Columns.Add("Price", typeof(decimal));
            cartTable.Columns.Add("Quantity", typeof(int));
            cartTable.Columns.Add("Total", typeof(decimal), "Price * Quantity");
        }

        private void BindTable()
        {
            dgvCart.DataSource = cartTable;

            // columns might not be created until bound — guard with try/catch
            try
            {
                if (dgvCart.Columns.Contains("MaMon"))
                    dgvCart.Columns["MaMon"].Visible = false;
                if (dgvCart.Columns.Contains("TenMon"))
                {
                    dgvCart.Columns["TenMon"].ReadOnly = true;
                    dgvCart.Columns["TenMon"].HeaderText = "Tên món";
                }
                if (dgvCart.Columns.Contains("Price"))
                {
                    dgvCart.Columns["Price"].ReadOnly = true;
                    dgvCart.Columns["Price"].HeaderText = "Đơn giá";
                    dgvCart.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (dgvCart.Columns.Contains("Quantity"))
                {
                    dgvCart.Columns["Quantity"].HeaderText = "SL";
                    dgvCart.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                if (dgvCart.Columns.Contains("Total"))
                {
                    dgvCart.Columns["Total"].HeaderText = "Thành tiền";
                    dgvCart.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvCart.Columns["Total"].ReadOnly = true;
                }
            }
            catch { /* ignore initial binding timing issues */ }

            // VNĐ formatting for cart grid
            TableStyleHelper.ApplyVndFormatting(dgvCart, "Price", "Total");

            dgvCart.CellEndEdit += DgvCart_CellEndEdit;

            if (!dgvCart.Columns.Contains("Remove"))
            {
                var btnCol = new DataGridViewButtonColumn
                {
                    Name = "Remove",
                    HeaderText = "Xóa",
                    Text = "Xóa",
                    UseColumnTextForButtonValue = true,
                    Width = 70,
                    FlatStyle = FlatStyle.Standard
                };
                dgvCart.Columns.Add(btnCol);
                dgvCart.CellClick += DgvCart_CellClick;
            }

            UpdateTotal();
            UpdateBadge();
        }

        private void DgvCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCart.Columns[e.ColumnIndex].Name == "Quantity")
            {
                var row = dgvCart.Rows[e.RowIndex];
                if (!int.TryParse(row.Cells["Quantity"].Value?.ToString() ?? "0", out int q) || q < 1)
                {
                    row.Cells["Quantity"].Value = 1;
                }
                UpdateTotal();
                UpdateBadge();
            }
        }

        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvCart.Columns[e.ColumnIndex].Name == "Remove")
            {
                int maMon = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["MaMon"].Value);
                RemoveItem(maMon);
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0m;
            foreach (DataRow r in cartTable.Rows)
            {
                total += Convert.ToDecimal(r["Total"]);
            }
            lblTotal.Text = "Tổng: " + TableStyleHelper.FormatVnd(total);
        }

        private int GetTotalQuantity()
        {
            int sum = 0;
            foreach (DataRow r in cartTable.Rows)
            {
                sum += Convert.ToInt32(r["Quantity"]);
            }
            return sum;
        }

        private void UpdateBadge()
        {
            int qty = GetTotalQuantity();
            if (qty <= 0)
            {
                lblBadge.Visible = false;
            }
            else
            {
                lblBadge.Text = qty.ToString();
                lblBadge.Visible = true;
            }
        }

        // Public API
        public void AddItem(int maMon, string tenMon, decimal price, int quantity = 1)
        {
            // If exists, increment quantity
            foreach (DataRow r in cartTable.Rows)
            {
                if (Convert.ToInt32(r["MaMon"]) == maMon)
                {
                    r["Quantity"] = Convert.ToInt32(r["Quantity"]) + quantity;
                    UpdateTotal();
                    UpdateBadge();
                    return;
                }
            }

            var newRow = cartTable.NewRow();
            newRow["MaMon"] = maMon;
            newRow["TenMon"] = tenMon;
            newRow["Price"] = price;
            newRow["Quantity"] = quantity;
            cartTable.Rows.Add(newRow);
            UpdateTotal();
            UpdateBadge();
        }

        public DataTable GetCartTable() => cartTable.Copy();

        public void RemoveItem(int maMon)
        {
            DataRow row = null;
            foreach (DataRow r in cartTable.Rows)
            {
                if (Convert.ToInt32(r["MaMon"]) == maMon) { row = r; break; }
            }
            if (row != null) cartTable.Rows.Remove(row);
            UpdateTotal();
            UpdateBadge();
        }

        // Clear cart
        public void Clear()
        {
            cartTable.Rows.Clear();
            UpdateTotal();
            UpdateBadge();
        }
    }
}

namespace PM_Ban_Do_An_Nhanh
{
    public class frmEditPrice : Form
    {
        private readonly NumericUpDown nudPrice;
        private readonly Button btnOk;
        private readonly Button btnCancel;

        public decimal NewPrice { get; private set; }

        public frmEditPrice(decimal currentPrice)
        {
            Text = "Chỉnh giá";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(320, 120);

            nudPrice = new NumericUpDown
            {
                Left = 16,
                Top = 16,
                Width = 280,
                Minimum = 0,
                Maximum = decimal.MaxValue,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Value = currentPrice < 0 ? 0 : currentPrice
            };

            btnOk = new Button
            {
                Text = "Lưu",
                Width = 90,
                Height = 30,
                Left = 116,
                Top = 64,
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Hủy",
                Width = 90,
                Height = 30,
                Left = 206,
                Top = 64,
                DialogResult = DialogResult.Cancel
            };

            btnOk.Click += (s, e) =>
            {
                NewPrice = nudPrice.Value;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(nudPrice);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
