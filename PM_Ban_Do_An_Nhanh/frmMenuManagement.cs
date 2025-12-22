using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Controls;
using PM_Ban_Do_An_Nhanh.Forms;
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

        // Added controls
        private CartControl cartControl;
        private ContextMenuStrip dgvContextMenu;

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

            // Ensure we handle column visibility after binding completes
            dgvMonAn.DataBindingComplete += DgvMonAn_DataBindingComplete;

            // Add context menu on the grid for quick actions (Edit price)
            dgvContextMenu = new ContextMenuStrip();
            dgvContextMenu.Items.Add("Edit price", null, DgvContext_EditPrice_Click);
            dgvMonAn.ContextMenuStrip = dgvContextMenu;

            // Add cart control (dock to right) so item cards can add to cart
            cartControl = new CartControl();
            cartControl.PlaceOrderClicked += CartControl_PlaceOrderClicked;
            this.Controls.Add(cartControl);
            cartControl.BringToFront();

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
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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
            var lblPrice = new Label { Text = $"{Convert.ToDecimal(itemRow["Gia"]):N0} VND", ForeColor = Color.DarkRed, Left = 8, Top = 132, Width = 100 };

            var nudQty = new NumericUpDown { Left = 170, Top = 128, Width = 44, Minimum = 1, Value = 1 };
            var btnAdd = new Button { Text = "Add", Left = 216, Top = 126, Width = 52, BackColor = Color.FromArgb(37, 150, 84), ForeColor = Color.White };

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

            flowLayoutPanelItems.Controls.Add(pnl);
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
                MessageBox.Show($"Placing order with {dt.Rows.Count} items. Total: {total:N0} VND");

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

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

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
                Dock = DockStyle.Top,
                Height = 360,
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
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnPlaceOrder = new Button
            {
                Text = "Place Order",
                AutoSize = true,
                BackColor = Color.FromArgb(37, 150, 84),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPlaceOrder.Click += (s, e) => PlaceOrderClicked?.Invoke(this, EventArgs.Empty);

            btnClear = new Button
            {
                Text = "Clear",
                AutoSize = true
            };
            btnClear.Click += (s, e) => Clear();

            fl.Controls.Add(btnPlaceOrder);
            fl.Controls.Add(btnClear);

            // We add controls in z-order: badge on top so bring to front after adding others
            this.Controls.Add(fl);
            this.Controls.Add(lblTotal);
            this.Controls.Add(dgvCart);
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
                    dgvCart.Columns["TenMon"].ReadOnly = true;
                if (dgvCart.Columns.Contains("Price"))
                    dgvCart.Columns["Price"].ReadOnly = true;
                if (dgvCart.Columns.Contains("Quantity"))
                    dgvCart.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            catch { /* ignore initial binding timing issues */ }

            dgvCart.CellEndEdit += DgvCart_CellEndEdit;

            if (!dgvCart.Columns.Contains("Remove"))
            {
                var btnCol = new DataGridViewButtonColumn
                {
                    Name = "Remove",
                    Text = "Remove",
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
            lblTotal.Text = $"Total: {total:N0} VND";
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
