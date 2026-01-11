using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Entities;
using PM_Ban_Do_An_Nhanh.UI;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmSales : Form
    {
        private MonAnBLL monAnBLL = new MonAnBLL();
        private DonHangBLL donHangBLL = new DonHangBLL();
        private KhachHangBLL khachHangBLL = new KhachHangBLL();

        private TabPage tabCart;
        private Panel cartPanel;
        private const string ColEditMinus = "__edit_minus";
        private const string ColEditPlus = "__edit_plus";
        private SplitContainer orderSplit;
        private Button btnEditOrder;
        private bool isEditOrderMode = false;
        private List<ChiTietDonHang> editSnapshot;

        private List<ChiTietDonHang> currentOrderItems = new List<ChiTietDonHang>();
        private KhachHang selectedCustomer = null;
        private decimal selectedCustomerTongChiTieu = 0m;
        private int? selectedMaDH = null;

        private readonly Dictionary<int, int> monAnStock = new Dictionary<int, int>();

        private bool isProcessingPayment = false;

        private bool isEnsuringEqualMainTabs = false;
        private bool isEnsureEqualMainTabsScheduled = false;


        private bool customerLayoutInitialized = false;
        private TextBox txtCustomerTen;
        private TextBox txtCustomerSdt;
        private Button btnCustomerLuu;
        private Button btnCustomerSua;
        private Button btnCustomerXoa;
        private Button btnCustomerHuy;
        private bool isEditingCustomer = false;
        private string editingCustomerSdt = null;

        private SplitContainer customerSplit;
        private const int CustomerPanelWidth = 360;

        private void AdjustCustomerSplitLayout()
        {
            if (customerSplit == null) return;
            if (customerSplit.IsDisposed) return;

            int available = customerSplit.ClientSize.Width - customerSplit.SplitterWidth;
            if (available <= 0) return;

            const int preferredMinRight = 420;
            const int preferredLeft = CustomerPanelWidth;

            try
            {
                customerSplit.Panel1MinSize = 0;
                customerSplit.Panel2MinSize = 0;
            }
            catch { }

            try
            {
                int pre = Math.Max(0, Math.Min(available, Math.Min(preferredLeft, available / 2)));
                if (customerSplit.SplitterDistance != pre)
                    customerSplit.SplitterDistance = pre;
            }
            catch { }

            if (available < (preferredLeft + preferredMinRight))
            {
                return;
            }

            try
            {
                customerSplit.Panel1MinSize = preferredLeft;
                customerSplit.Panel2MinSize = preferredMinRight;
            }
            catch
            {
                return;
            }

            int minDistance = preferredLeft;
            int maxDistance = available - preferredMinRight;
            if (maxDistance < minDistance) return;

            int desired = Math.Max(minDistance, Math.Min(maxDistance, preferredLeft));
            try
            {
                if (customerSplit.SplitterDistance != desired)
                    customerSplit.SplitterDistance = desired;
            }
            catch { }
        }

        private (string Rank, decimal DiscountRate) GetRankByTotalSpent(decimal tongChiTieu)
        {
            // Rank by total spent (TongChiTieu)
            // Member: 0 - 500k (2%)
            // Silver: > 500k (5%)
            // Gold: > 2m (8%)
            // Platinum: > 5m (12%)
            // Diamond: > 8m (15%)
            if (tongChiTieu > 8000000m) return ("Kim Cương", 0.15m);
            if (tongChiTieu > 5000000m) return ("Bạch Kim", 0.12m);
            if (tongChiTieu > 2000000m) return ("Vàng", 0.08m);
            if (tongChiTieu > 500000m) return ("Bạc", 0.05m);
            return ("Thành Viên", 0.02m);
        }
        private DateTime lastPaymentTime = DateTime.MinValue;

        public frmSales()
        {
            InitializeComponent();
            Text = "Bán hàng";

            EnsureUiLayout();

            Load += frmSales_Load;

            // NOTE: Click/CellClick handlers are wired in frmSales.Designer.cs.
            // Avoid re-wiring here to prevent duplicate execution (e.g. double confirmation dialogs).

            EnsureEditOrderButton();
            UpdateOrderEditUi();

            mainTabControl.SelectedIndexChanged -= mainTabControl_SelectedIndexChanged;
            mainTabControl.SelectedIndexChanged += mainTabControl_SelectedIndexChanged;

            if (mainTabControl != null)
            {
                mainTabControl.Resize -= MainTabControl_Resize;
                mainTabControl.Resize += MainTabControl_Resize;
                mainTabControl.ControlAdded -= MainTabControl_ControlAdded;
                mainTabControl.ControlAdded += MainTabControl_ControlAdded;
                mainTabControl.ControlRemoved -= MainTabControl_ControlRemoved;
                mainTabControl.ControlRemoved += MainTabControl_ControlRemoved;
            }
        }

        private void MainTabControl_Resize(object sender, EventArgs e)
        {
            ScheduleEnsureEqualMainTabs();
        }

        private void MainTabControl_ControlAdded(object sender, ControlEventArgs e)
        {
            ScheduleEnsureEqualMainTabs();
        }

        private void MainTabControl_ControlRemoved(object sender, ControlEventArgs e)
        {
            ScheduleEnsureEqualMainTabs();
        }

        private void ScheduleEnsureEqualMainTabs()
        {
            if (isEnsureEqualMainTabsScheduled) return;
            if (IsDisposed) return;
            if (mainTabControl == null || mainTabControl.IsDisposed) return;

            isEnsureEqualMainTabsScheduled = true;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    isEnsureEqualMainTabsScheduled = false;
                    EnsureEqualMainTabs();
                }));
            }
            catch
            {
                isEnsureEqualMainTabsScheduled = false;
            }
        }

        private void EnsureUiLayout()
        {
            EnsureEditOrderButton();
            EnsureCartTab();
            EnsureCartPanel();

            EnsureEqualMainTabs();

            if (pnlMonAn != null)
            {
                pnlMonAn.FlowDirection = FlowDirection.TopDown;
                pnlMonAn.WrapContents = false;
            }

            EnsureOrderTabLayout();
            EnsureCustomerTabLayout();
            EnsureHistoryTabLayout();
        }

        private void EnsureEqualMainTabs()
        {
            if (mainTabControl == null) return;
            if (mainTabControl.IsDisposed) return;
            if (mainTabControl.TabPages == null) return;

            if (isEnsuringEqualMainTabs) return;
            isEnsuringEqualMainTabs = true;
            try
            {
                int tabCount = mainTabControl.TabPages.Count;
                if (tabCount <= 0) return;

                // Ensure fixed-size tabs so headers can be equal width
                if (mainTabControl.SizeMode != TabSizeMode.Fixed)
                    mainTabControl.SizeMode = TabSizeMode.Fixed;

                // Heuristic: use available width; keep a minimum so text isn't completely clipped
                int available = mainTabControl.ClientSize.Width - 8;
                if (available <= 0)
                {
                    available = mainTabControl.Width - 8;
                    if (available <= 0) return;
                }

                int w = available / tabCount;
                w = Math.Max(110, w);
                int h = Math.Max(28, mainTabControl.ItemSize.Height);

                var newSize = new Size(w, h);
                if (newSize.Width <= 0 || newSize.Height <= 0) return;

                if (mainTabControl.ItemSize != newSize)
                    mainTabControl.ItemSize = newSize;
            }
            catch
            {
                // Avoid crashing the UI thread if WinForms is in an intermediate layout state.
            }
            finally
            {
                isEnsuringEqualMainTabs = false;
            }
        }

        private void EnsureCartTab()
        {
            if (mainTabControl == null) return;

            if (tabCart == null)
            {
                tabCart = new TabPage("Giỏ hàng")
                {
                    BackColor = SystemColors.Control,
                    Padding = new Padding(16)
                };
            }

            if (!mainTabControl.TabPages.Contains(tabCart))
            {
                int historyIndex = mainTabControl.TabPages.IndexOf(tabHistory);
                if (historyIndex < 0) historyIndex = mainTabControl.TabPages.Count;

                // place next to "Lịch sử" (after it). If history is last, Insert will behave like Add.
                int insertAt = Math.Min(mainTabControl.TabPages.Count, historyIndex + 1);
                mainTabControl.TabPages.Insert(insertAt, tabCart);
            }
        }

        private void EnsureCartPanel()
        {
            if (cartPanel != null) return;
            if (dgvOrderList == null) return;

            cartPanel = new Panel { Dock = DockStyle.Fill };

            dgvOrderList.Dock = DockStyle.Fill;
            cartPanel.Controls.Add(dgvOrderList);

            var bottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                Padding = new Padding(12)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var info = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            lblTenKhachHang.AutoSize = true;
            lblTongTien.AutoSize = true;
            info.Controls.Add(lblTenKhachHang);
            info.Controls.Add(lblTongTien);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true
            };
            actions.Controls.Add(btnThanhToan);
            actions.Controls.Add(btnHuyDon);
            actions.Controls.Add(btnRemoveItem);
            actions.Controls.Add(btnEditOrder);
            actions.Controls.Add(btnMinusItem);
            actions.Controls.Add(btnPlusItem);

            grid.Controls.Add(info, 0, 0);
            grid.Controls.Add(actions, 1, 0);
            bottom.Controls.Add(grid);
            cartPanel.Controls.Add(bottom);
        }

        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            MoveCartPanelToSelectedTab();
        }

        private void MoveCartPanelToSelectedTab()
        {
            if (cartPanel == null || mainTabControl == null) return;

            Control targetParent = null;
            if (mainTabControl.SelectedTab == tabCart)
            {
                targetParent = tabCart;
            }
            else
            {
                targetParent = orderSplit?.Panel2;
            }

            if (targetParent == null) return;

            if (cartPanel.Parent != targetParent)
            {
                try
                {
                    cartPanel.Parent?.Controls.Remove(cartPanel);
                }
                catch { }

                targetParent.Controls.Add(cartPanel);
                cartPanel.Dock = DockStyle.Fill;
                cartPanel.BringToFront();
            }
        }

        private void EnsureEditOrderButton()
        {
            if (btnEditOrder != null) return;

            btnEditOrder = new Button
            {
                Text = "Sửa",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            btnEditOrder.Click += btnEditOrder_Click;

            // Standardize action button texts and sizes
            btnHuyDon.Text = "Đặt lại";
            btnRemoveItem.Text = "Xóa";
            btnThanhToan.Text = "Thanh toán";

            btnRemoveItem.BackColor = Color.FromArgb(220, 53, 69);
            btnRemoveItem.ForeColor = Color.White;
            btnRemoveItem.FlatStyle = FlatStyle.Flat;
            btnRemoveItem.FlatAppearance.BorderSize = 0;

            btnHuyDon.BackColor = Color.FromArgb(255, 193, 7);
            btnHuyDon.ForeColor = Color.Black;
            btnHuyDon.FlatStyle = FlatStyle.Flat;
            btnHuyDon.FlatAppearance.BorderSize = 0;

            btnThanhToan.BackColor = Color.FromArgb(0, 123, 255);
            btnThanhToan.ForeColor = Color.White;
            btnThanhToan.FlatStyle = FlatStyle.Flat;
            btnThanhToan.FlatAppearance.BorderSize = 0;

            btnHuyDon.AutoSize = true;
            btnHuyDon.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRemoveItem.AutoSize = true;
            btnRemoveItem.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnThanhToan.AutoSize = true;
            btnThanhToan.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            btnPlusItem.Width = 42;
            btnMinusItem.Width = 42;

            btnPlusItem.Visible = false;
            btnMinusItem.Visible = false;
        }

        private void UpdateOrderEditUi()
        {
            if (btnEditOrder == null) return;

            bool hasSelectedRow = dgvOrderList != null && dgvOrderList.CurrentRow != null && dgvOrderList.CurrentRow.Index >= 0;

            // Edit +/- is handled per-row in the DataGridView.
            btnPlusItem.Visible = false;
            btnMinusItem.Visible = false;

            btnEditOrder.Text = isEditOrderMode ? "Xác nhận" : "Sửa";
            btnEditOrder.Enabled = (currentOrderItems != null && currentOrderItems.Count > 0) && (isEditOrderMode || hasSelectedRow);

            // Khi đang sửa thì không cho thanh toán để tránh nhầm
            btnThanhToan.Enabled = !isEditOrderMode;

            EnsureInlineEditColumns(isEditOrderMode);
        }

        private void EnsureInlineEditColumns(bool enabled)
        {
            if (dgvOrderList == null) return;

            bool hasMinus = dgvOrderList.Columns.Contains(ColEditMinus);
            bool hasPlus = dgvOrderList.Columns.Contains(ColEditPlus);

            if (!enabled)
            {
                if (hasMinus) dgvOrderList.Columns.Remove(ColEditMinus);
                if (hasPlus) dgvOrderList.Columns.Remove(ColEditPlus);
                return;
            }

            if (!hasMinus)
            {
                var colMinus = new DataGridViewButtonColumn
                {
                    Name = ColEditMinus,
                    HeaderText = "",
                    Text = "-",
                    UseColumnTextForButtonValue = true,
                    Width = 36,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvOrderList.Columns.Add(colMinus);
            }

            if (!hasPlus)
            {
                var colPlus = new DataGridViewButtonColumn
                {
                    Name = ColEditPlus,
                    HeaderText = "",
                    Text = "+",
                    UseColumnTextForButtonValue = true,
                    Width = 36,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None
                };
                dgvOrderList.Columns.Add(colPlus);
            }

            // Keep +/- at the end of the row (after other data columns)
            if (dgvOrderList.Columns.Contains(ColEditMinus))
                dgvOrderList.Columns[ColEditMinus].DisplayIndex = dgvOrderList.Columns.Count - 2;
            if (dgvOrderList.Columns.Contains(ColEditPlus))
                dgvOrderList.Columns[ColEditPlus].DisplayIndex = dgvOrderList.Columns.Count - 1;

            dgvOrderList.CellContentClick -= DgvOrderList_CellContentClick;
            dgvOrderList.CellContentClick += DgvOrderList_CellContentClick;
        }

        private void DgvOrderList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!isEditOrderMode) return;
            if (e.RowIndex < 0) return;

            var colName = dgvOrderList.Columns[e.ColumnIndex]?.Name;
            if (colName != ColEditMinus && colName != ColEditPlus) return;

            int maMon = Convert.ToInt32(dgvOrderList.Rows[e.RowIndex].Cells["MaMon"].Value);
            var item = currentOrderItems.FirstOrDefault(x => x.MaMon == maMon);
            if (item == null) return;

            if (colName == ColEditPlus)
            {
                if (monAnStock.TryGetValue(maMon, out int stock) && stock != int.MaxValue)
                {
                    if (item.SoLuong + 1 > stock)
                    {
                        MessageBox.Show($"Tồn kho '{item.TenMon}' chỉ còn {stock}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                item.SoLuong++;
            }
            else
            {
                item.SoLuong--;
                if (item.SoLuong <= 0)
                    currentOrderItems.Remove(item);
            }

            RefreshOrderGrid();
        }

        private void btnEditOrder_Click(object sender, EventArgs e)
        {
            if (!isEditOrderMode)
            {
                if (currentOrderItems == null || currentOrderItems.Count == 0)
                {
                    MessageBox.Show("Giỏ hàng đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Snapshot để có thể hoàn tác nếu người dùng bấm Hủy
                editSnapshot = currentOrderItems
                    .Select(i => new ChiTietDonHang
                    {
                        MaDH = i.MaDH,
                        MaMon = i.MaMon,
                        TenMon = i.TenMon,
                        SoLuong = i.SoLuong,
                        DonGia = i.DonGia
                    })
                    .ToList();

                isEditOrderMode = true;
                UpdateOrderEditUi();
                return;
            }

            // Xác nhận sửa
            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn cập nhật số lượng món trong giỏ hàng không?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                // Hủy: quay về giỏ hàng trước khi sửa
                if (editSnapshot != null)
                {
                    currentOrderItems = editSnapshot
                        .Select(i => new ChiTietDonHang
                        {
                            MaDH = i.MaDH,
                            MaMon = i.MaMon,
                            TenMon = i.TenMon,
                            SoLuong = i.SoLuong,
                            DonGia = i.DonGia
                        })
                        .ToList();

                    RefreshOrderGrid();
                }
            }

            ExitEditOrderMode();
        }

        private void ExitEditOrderMode()
        {
            isEditOrderMode = false;
            editSnapshot = null;
            UpdateOrderEditUi();
        }

        private void AdjustOrderSplitLayout()
        {
            if (orderSplit == null) return;

            // Keep the right panel (cart/order list) visible by enforcing a minimum width
            const int preferredMinRight = 580;
            const int preferredRight = 700;
            const int preferredMinLeft = 420;

            const int absoluteMinLeft = 200;
            const int absoluteMinRight = 200;

            int available = orderSplit.ClientSize.Width - orderSplit.SplitterWidth;
            if (available <= 0) return;

            // If container is too small, don't enforce big min sizes (avoids invalid ranges)
            if (available < (absoluteMinLeft + absoluteMinRight))
            {
                orderSplit.Panel1MinSize = 0;
                orderSplit.Panel2MinSize = 0;

                // SplitterDistance must be within [0, available]
                try
                {
                    int safeDistance = Math.Max(0, Math.Min(available, available / 2));
                    orderSplit.SplitterDistance = safeDistance;
                }
                catch { }
                return;
            }

            int minRight = Math.Min(preferredMinRight, Math.Max(absoluteMinRight, available / 3));
            int minLeft = Math.Min(preferredMinLeft, Math.Max(absoluteMinLeft, available - minRight));

            orderSplit.Panel2MinSize = minRight;
            orderSplit.Panel1MinSize = minLeft;

            int minDistance = orderSplit.Panel1MinSize;
            int maxDistance = available - orderSplit.Panel2MinSize;
            if (maxDistance < minDistance) return;

            int targetRight = Math.Min(preferredRight, available - minDistance);
            targetRight = Math.Max(orderSplit.Panel2MinSize, targetRight);

            int distance = available - targetRight;
            distance = Math.Max(minDistance, Math.Min(maxDistance, distance));

            try { orderSplit.SplitterDistance = distance; } catch { }
        }

        private void EnsureOrderTabLayout()
        {
            if (tabOrder == null || pnlMonAn == null) return;
            if (orderSplit != null && orderSplit.Parent != null) return;

            tabOrder.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
                SplitterWidth = 6
            };

            orderSplit = split;
            split.SizeChanged += (s, e) => AdjustOrderSplitLayout();
            split.HandleCreated += (s, e) =>
            {
                try
                {
                    BeginInvoke(new Action(AdjustOrderSplitLayout));
                }
                catch { }
            };

            var left = new Panel { Dock = DockStyle.Fill };
            pnlMonAn.Dock = DockStyle.Fill;
            left.Controls.Add(pnlMonAn);
            split.Panel1.Controls.Add(left);

            EnsureCartPanel();
            split.Panel2.Controls.Add(cartPanel);
            cartPanel.Dock = DockStyle.Fill;

            tabOrder.Controls.Add(split);

            try
            {
                BeginInvoke(new Action(AdjustOrderSplitLayout));
            }
            catch { }

            MoveCartPanelToSelectedTab();
        }

        private void EnsureCustomerTabLayout()
        {
            if (tabCustomer == null || dgvKhachHang == null) return;
            if (customerLayoutInitialized) return;

            tabCustomer.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel1,
                SplitterWidth = 6
            };
            customerSplit = split;
            split.IsSplitterFixed = true;
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;

            split.HandleCreated += (s, e) =>
            {
                try { BeginInvoke(new Action(AdjustCustomerSplitLayout)); } catch { }
            };
            split.SizeChanged += (s, e) => AdjustCustomerSplitLayout();

            var left = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                AutoScroll = true
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var lblTen = new Label
            {
                Text = "Tên khách hàng",
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 4)
            };
            txtCustomerTen = new TextBox
            {
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 12)
            };

            var lblSdt = new Label
            {
                Text = "Số điện thoại",
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 4)
            };
            txtCustomerSdt = new TextBox
            {
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 16)
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0)
            };

            btnCustomerLuu = new Button { Text = "Lưu", Width = 220, Height = 36, Margin = new Padding(0, 0, 0, 10) };
            btnCustomerSua = new Button { Text = "Sửa", Width = 220, Height = 36, Margin = new Padding(0, 0, 0, 10) };
            btnCustomerXoa = new Button { Text = "Xóa", Width = 220, Height = 36, Margin = new Padding(0, 0, 0, 10) };
            btnCustomerHuy = new Button { Text = "Hủy", Width = 220, Height = 36, Margin = new Padding(0, 0, 0, 0) };

            btnCustomerLuu.Click += BtnCustomerLuu_Click;
            btnCustomerSua.Click += BtnCustomerSua_Click;
            btnCustomerXoa.Click += BtnCustomerXoa_Click;
            btnCustomerHuy.Click += BtnCustomerHuy_Click;

            buttons.Controls.Add(btnCustomerLuu);
            buttons.Controls.Add(btnCustomerSua);
            buttons.Controls.Add(btnCustomerXoa);
            buttons.Controls.Add(btnCustomerHuy);

            formLayout.Controls.Add(lblTen, 0, 0);
            formLayout.Controls.Add(txtCustomerTen, 0, 1);
            formLayout.Controls.Add(lblSdt, 0, 2);
            formLayout.Controls.Add(txtCustomerSdt, 0, 3);
            formLayout.Controls.Add(buttons, 0, 4);

            left.Controls.Add(formLayout);

            dgvKhachHang.Dock = DockStyle.Fill;
            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhachHang.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKhachHang.RowHeadersVisible = false;

            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(dgvKhachHang);
            tabCustomer.Controls.Add(split);

            AdjustCustomerSplitLayout();

            customerLayoutInitialized = true;
        }

        private void ResetCustomerEditor()
        {
            isEditingCustomer = false;
            editingCustomerSdt = null;

            if (txtCustomerTen != null) txtCustomerTen.Text = "";
            if (txtCustomerSdt != null)
            {
                txtCustomerSdt.Text = "";
                txtCustomerSdt.Enabled = true;
            }
        }

        private void BtnCustomerLuu_Click(object sender, EventArgs e)
        {
            string ten = txtCustomerTen?.Text?.Trim() ?? "";
            string sdt = txtCustomerSdt?.Text?.Trim() ?? "";
            string diaChi = "";

            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(sdt))
            {
                MessageBox.Show("Vui lòng nhập Tên khách hàng và Số điện thoại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (isEditingCustomer)
                {
                    // Update by original SDT (do not allow SDT change in this simple editor)
                    bool ok = khachHangBLL.CapNhatKhachHang(ten, editingCustomerSdt ?? sdt, diaChi);
                    if (!ok)
                    {
                        MessageBox.Show("Cập nhật khách hàng thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    if (khachHangBLL.LayThongTinKhachHangBySDT(sdt) != null)
                    {
                        MessageBox.Show("Số điện thoại này đã tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    bool ok = khachHangBLL.ThemKhachHang(ten, sdt, diaChi);
                    if (!ok)
                    {
                        MessageBox.Show("Thêm khách hàng thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                LoadKhachHang();
                SelectCustomerBySdt(sdt);
                ResetCustomerEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCustomerSua_Click(object sender, EventArgs e)
        {
            if (dgvKhachHang == null || dgvKhachHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var row = dgvKhachHang.CurrentRow;
                string ten = row.Cells["TenKH"].Value?.ToString() ?? "";
                string sdt = row.Cells["SDT"].Value?.ToString() ?? "";

                if (txtCustomerTen != null) txtCustomerTen.Text = ten;
                if (txtCustomerSdt != null)
                {
                    txtCustomerSdt.Text = sdt;
                    txtCustomerSdt.Enabled = false;
                }

                isEditingCustomer = true;
                editingCustomerSdt = sdt;
            }
            catch
            {
            }
        }

        private void BtnCustomerXoa_Click(object sender, EventArgs e)
        {
            if (dgvKhachHang == null || dgvKhachHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string sdt = dgvKhachHang.CurrentRow.Cells["SDT"].Value?.ToString() ?? "";
                string ten = dgvKhachHang.CurrentRow.Cells["TenKH"].Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(sdt)) return;

                if (MessageBox.Show($"Xóa khách hàng {ten} (SĐT: {sdt})?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                bool ok = khachHangBLL.XoaKhachHang(sdt);
                if (!ok)
                {
                    MessageBox.Show("Xóa khách hàng thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadKhachHang();
                ResetCustomerEditor();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCustomerHuy_Click(object sender, EventArgs e)
        {
            ResetCustomerEditor();
        }

        private void SelectCustomerBySdt(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return;
            if (dgvKhachHang == null) return;

            try
            {
                if (!dgvKhachHang.Columns.Contains("SDT")) return;

                foreach (DataGridViewRow row in dgvKhachHang.Rows)
                {
                    if (row == null) continue;
                    var cell = row.Cells["SDT"];
                    if (cell == null) continue;

                    object v = cell.Value;
                    if (v == null) continue;
                    if (v == DBNull.Value) continue;

                    if (string.Equals(v.ToString().Trim(), sdt.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        int colIndex = dgvKhachHang.Columns.Contains("TenKH") ? dgvKhachHang.Columns["TenKH"].Index : 0;
                        dgvKhachHang.CurrentCell = row.Cells[colIndex];
                        row.Selected = true;
                        dgvKhachHang_CellClick(dgvKhachHang, new DataGridViewCellEventArgs(colIndex, row.Index));
                        break;
                    }
                }
            }
            catch
            {
            }
        }

        private void EnsureHistoryTabLayout()
        {
            if (tabHistory == null || dgvHoaDon == null) return;
            if (tabHistory.Controls.Contains(btnPrint) || tabHistory.Controls.Contains(btnExportPdf))
            {
                if (!tabHistory.Controls.Contains(dgvHoaDon))
                {
                    dgvHoaDon.Dock = DockStyle.Fill;
                    tabHistory.Controls.Add(dgvHoaDon);
                }
                return;
            }

            tabHistory.Controls.Clear();

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(12, 10, 12, 10)
            };

            top.Controls.Add(btnPrint);
            top.Controls.Add(btnExportPdf);

            dgvHoaDon.Dock = DockStyle.Fill;
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHoaDon.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            tabHistory.Controls.Add(dgvHoaDon);
            tabHistory.Controls.Add(top);
        }

        // ================= LOAD =================
        private async void frmSales_Load(object sender, EventArgs e)
        {
            SetupOrderGrid();
            ClearOrder();

            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHoaDon.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // Load data asynchronously to avoid UI freeze
            await LoadMonAnAsync();
            LoadKhachHang();
            LoadHoaDon();

            pnlMonAn.SizeChanged -= PnlMonAn_SizeChanged;
            pnlMonAn.SizeChanged += PnlMonAn_SizeChanged;
        }

        private void PnlMonAn_SizeChanged(object sender, EventArgs e)
        {
            AdjustMenuCardLayout();
        }

        private void AdjustMenuCardLayout()
        {
            if (pnlMonAn == null) return;
            if (pnlMonAn.Controls.Count == 0) return;

            const int gap = 10;

            int available = pnlMonAn.ClientSize.Width - pnlMonAn.Padding.Left - pnlMonAn.Padding.Right;
            if (available <= 0) return;

            // reserve scrollbar space when needed
            if (pnlMonAn.VerticalScroll.Visible)
                available -= SystemInformation.VerticalScrollBarWidth;

            int cardWidth = Math.Max(0, available);

            foreach (Control c in pnlMonAn.Controls)
            {
                if (c is MenuItemCard card)
                {
                    // Force 1 column (full width) for easier browsing
                    card.Width = cardWidth;
                    card.Height = 104;
                    card.Margin = new Padding(gap);
                }
            }
        }

        // ================= MÓN ĂN =================
        private async Task LoadMonAnAsync()
        {
            pnlMonAn.Controls.Clear();
            monAnStock.Clear();

            try
            {
                DataTable dt = await Task.Run(() => monAnBLL.HienThiDanhSachMonAn());
                if (dt == null || dt.Rows.Count == 0)
                {
                    // Try to load sample menu items from Images/MenuItems if DB is empty
                    string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "MenuItems");
                    if (Directory.Exists(imagesFolder))
                    {
                        var files = Directory.GetFiles(imagesFolder)
                                             .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                                      || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                                      || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                             .ToArray();

                        if (files.Length > 0)
                        {
                            foreach (var file in files)
                            {
                                try
                                {
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    // Convert file name to friendly menu name (replace underscores/dashes)
                                    string friendlyName = fileName.Replace('_', ' ').Replace('-', ' ');
                                    // Default price heuristic: if file name contains digits use them else random
                                    decimal price = 20000m;
                                    // try parse trailing number
                                    var digits = new string(friendlyName.Where(char.IsDigit).ToArray());
                                    if (!string.IsNullOrEmpty(digits) && decimal.TryParse(digits, out decimal p))
                                    {
                                        price = p;
                                    }

                                    MenuItemCard card = new MenuItemCard();
                                    card.SetData(0, friendlyName, price, Path.Combine("Images", "MenuItems", Path.GetFileName(file)));
                                    card.AddClicked += (s, ev) => { AddItemToOrder(ev.MaMon, ev.TenMon, ev.Price, ev.Quantity); };
                                    pnlMonAn.Controls.Add(card);
                                }
                                catch (Exception exFile)
                                {
                                    Logger.Log(exFile);
                                }
                            }

                            return;
                        }
                    }

                    // show placeholder label if no images and no DB data
                    var lbl = new Label { Text = "Không có món ăn", AutoSize = true, ForeColor = Color.Gray };
                    pnlMonAn.Controls.Add(lbl);
                    return;
                }

                foreach (DataRow r in dt.Rows)
                {
                    try
                    {
                        int maMon = Convert.ToInt32(r["MaMon"]);
                        int stock = int.MaxValue;
                        if (dt.Columns.Contains("SoLuongTon"))
                        {
                            stock = r["SoLuongTon"] == DBNull.Value ? 0 : Convert.ToInt32(r["SoLuongTon"]);
                        }

                        monAnStock[maMon] = stock;

                        MenuItemCard card = new MenuItemCard();
                        card.SetData(
                            maMon,
                            r["TenMon"].ToString(),
                            Convert.ToDecimal(r["Gia"]),
                            r.Table.Columns.Contains("HinhAnh") ? r["HinhAnh"]?.ToString() : null
                        );

                        card.AddClicked += (s, ev) =>
                        {
                            AddItemToOrder(ev.MaMon, ev.TenMon, ev.Price, ev.Quantity);
                        };

                        pnlMonAn.Controls.Add(card);
                    }
                    catch (Exception exRow)
                    {
                        Logger.Log(exRow);
                        // continue with next row
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Không tải được danh sách món: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            AdjustMenuCardLayout();
        }

        // ================= ORDER =================
        private void SetupOrderGrid()
        {
            dgvOrderList.Columns.Clear();
            dgvOrderList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrderList.ReadOnly = true;
            dgvOrderList.AllowUserToAddRows = false;

            // Softer selection highlight (avoid strong blue)
            dgvOrderList.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 235, 235);
            dgvOrderList.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvOrderList.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 235, 235);
            dgvOrderList.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgvOrderList.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgvOrderList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvOrderList.GridColor = Color.FromArgb(220, 220, 220);
            dgvOrderList.EnableHeadersVisualStyles = false;

            // Prevent header from turning blue when selecting cells
            var headerBack = dgvOrderList.ColumnHeadersDefaultCellStyle.BackColor;
            var headerFore = dgvOrderList.ColumnHeadersDefaultCellStyle.ForeColor;
            if (headerBack.IsEmpty) headerBack = Color.FromArgb(245, 245, 245);
            if (headerFore.IsEmpty) headerFore = Color.Black;
            dgvOrderList.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBack;
            dgvOrderList.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFore;

            dgvOrderList.SelectionChanged -= DgvOrderList_SelectionChanged;
            dgvOrderList.SelectionChanged += DgvOrderList_SelectionChanged;

            dgvOrderList.Columns.Add("MaMon", "Mã");
            dgvOrderList.Columns["MaMon"].Visible = false;
            dgvOrderList.Columns.Add("TenMon", "Tên món");
            dgvOrderList.Columns.Add("SoLuong", "SL");
            dgvOrderList.Columns.Add("DonGia", "Đơn giá");
            dgvOrderList.Columns.Add("ThanhTien", "Thành tiền");

            if (dgvOrderList.Columns.Contains("DonGia"))
                dgvOrderList.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (dgvOrderList.Columns.Contains("ThanhTien"))
                dgvOrderList.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // VNĐ formatting for monetary columns
            TableStyleHelper.ApplyVndFormatting(dgvOrderList, "DonGia", "ThanhTien");

            EnsureInlineEditColumns(isEditOrderMode);
        }

        private void DgvOrderList_SelectionChanged(object sender, EventArgs e)
        {
            UpdateOrderEditUi();
        }

        private void AddItemToOrder(int maMon, string tenMon, decimal gia, int soLuong)
        {
            if (monAnStock.TryGetValue(maMon, out int stock) && stock != int.MaxValue)
            {
                int inCart = currentOrderItems.FirstOrDefault(x => x.MaMon == maMon)?.SoLuong ?? 0;
                int canAdd = stock - inCart;
                if (canAdd <= 0)
                {
                    MessageBox.Show($"'{tenMon}' đã hết tồn kho.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (soLuong > canAdd)
                {
                    MessageBox.Show($"Tồn kho '{tenMon}' còn {stock}. Trong giỏ đã có {inCart}. Chỉ có thể thêm {canAdd}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    soLuong = canAdd;
                }
            }

            ChiTietDonHang item = currentOrderItems.FirstOrDefault(x => x.MaMon == maMon);
            if (item != null)
                item.SoLuong += soLuong;
            else
                currentOrderItems.Add(new ChiTietDonHang
                {
                    MaMon = maMon,
                    TenMon = tenMon,
                    SoLuong = soLuong,
                    DonGia = gia
                });

            RefreshOrderGrid();
        }

        private void RefreshOrderGrid()
        {
            dgvOrderList.Rows.Clear();
            decimal total = 0;

            foreach (var i in currentOrderItems)
            {
                decimal tt = i.SoLuong * i.DonGia;

                // Add row and populate by column name to avoid column-order issues
                // when edit button columns are added/removed.
                int rowIndex = dgvOrderList.Rows.Add();
                var row = dgvOrderList.Rows[rowIndex];
                row.Cells["MaMon"].Value = i.MaMon;
                row.Cells["TenMon"].Value = i.TenMon;
                row.Cells["SoLuong"].Value = i.SoLuong;
                row.Cells["DonGia"].Value = i.DonGia;
                row.Cells["ThanhTien"].Value = tt;
                total += tt;
            }

            // auto-select a row so actions like "Sửa" can enable after adding items
            if (dgvOrderList.Rows.Count > 0 && (dgvOrderList.CurrentCell == null || dgvOrderList.CurrentCell.RowIndex < 0))
            {
                try
                {
                    dgvOrderList.CurrentCell = dgvOrderList.Rows[0].Cells["TenMon"];
                }
                catch { }
            }

            lblTongTien.Text = "Tổng tiền: " + TableStyleHelper.FormatVnd(total);
            UpdateOrderEditUi();
        }

        private void ClearOrder()
        {
            currentOrderItems.Clear();
            RefreshOrderGrid();
            selectedCustomer = null;
            lblTenKhachHang.Text = "Loại khách: Khách lẻ";
            selectedCustomerTongChiTieu = 0m;
            ExitEditOrderMode();
        }

        // ================= BUTTON ORDER =================
        private void btnPlusItem_Click(object sender, EventArgs e)
        {
            if (!isEditOrderMode) return;
            if (dgvOrderList.CurrentRow == null) return;
            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            currentOrderItems.First(x => x.MaMon == maMon).SoLuong++;
            RefreshOrderGrid();
        }

        private void btnMinusItem_Click(object sender, EventArgs e)
        {
            if (!isEditOrderMode) return;
            if (dgvOrderList.CurrentRow == null) return;
            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            ChiTietDonHang item = currentOrderItems.First(x => x.MaMon == maMon);
            if (item.SoLuong > 1)
                item.SoLuong--;
            else
                currentOrderItems.RemoveAll(x => x.MaMon == maMon);
            RefreshOrderGrid();
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvOrderList.CurrentRow == null) return;

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa món đang chọn khỏi giỏ hàng không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            currentOrderItems.RemoveAll(x => x.MaMon == maMon);
            RefreshOrderGrid();
        }

        private void btnHuyDon_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đặt lại giỏ hàng không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                ClearOrder();
        }

        // ================= THANH TOÁN =================
        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (isEditOrderMode)
            {
                MessageBox.Show("Bạn đang ở chế độ sửa. Vui lòng bấm 'Xác nhận' để hoàn tất sửa trước khi thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (isProcessingPayment) return;
            isProcessingPayment = true;

            try
            {
                if (currentOrderItems.Count == 0)
                {
                    MessageBox.Show("Chưa có món!");
                    return;
                }

                decimal tongTienGoc = currentOrderItems.Sum(x => x.SoLuong * x.DonGia);

                decimal tongChiTieuTruoc = selectedCustomer != null ? selectedCustomerTongChiTieu : 0m;
                var rankInfo = GetRankByTotalSpent(tongChiTieuTruoc);
                decimal discount = 0m;

                if (selectedCustomer != null && rankInfo.DiscountRate > 0)
                {
                    discount = Math.Round(tongTienGoc * rankInfo.DiscountRate, 0, MidpointRounding.AwayFromZero);
                }

                decimal tongTien = tongTienGoc - discount;

                DonHang dh = new DonHang
                {
                    NgayLap = DateTime.Now,
                    TongTien = tongTien,
                    TrangThaiThanhToan = "Đã thanh toán",
                    MaKH = selectedCustomer != null ? (int?)selectedCustomer.MaKH : null
                };

                int maDH = donHangBLL.ThemDonHang(dh, currentOrderItems);

                string message;
                if (selectedCustomer != null)
                {
                    decimal tongChiTieuSau = tongChiTieuTruoc + tongTien;
                    var newRankInfo = GetRankByTotalSpent(tongChiTieuSau);

                    message = "Thanh toán thành công!";
                    message += $"\nKhách: {selectedCustomer.TenKH} ({rankInfo.Rank})";

                    if (discount > 0)
                        message += $"\nGiảm giá: {TableStyleHelper.FormatVnd(discount)}";

                    message += $"\nTổng thanh toán: {TableStyleHelper.FormatVnd(tongTien)}";

                    message += $"\nTổng đã chi: {TableStyleHelper.FormatVnd(tongChiTieuSau)}";

                    if (!string.Equals(rankInfo.Rank, newRankInfo.Rank, StringComparison.OrdinalIgnoreCase))
                    {
                        message += $"\n\nChúc mừng! Bạn đã thăng hạng lên: {newRankInfo.Rank} (Giảm {(int)(newRankInfo.DiscountRate * 100)}%).";
                    }
                }
                else
                {
                    message = "Thanh toán thành công!";
                }

                MessageBox.Show(message);
                InHoaDon(maDH);
                ClearOrder();
                LoadHoaDon();
                LoadKhachHang();

                try { _ = LoadMonAnAsync(); } catch { }
            }
            finally
            {
                isProcessingPayment = false;
            }
        }

        // ================= KHÁCH HÀNG =================
        private void LoadKhachHang()
        {
            dgvKhachHang.DataSource = khachHangBLL.HienThiDanhSachKhachHangTomTat();
            dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhachHang.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            if (dgvKhachHang.Columns.Contains("MaKH"))
            {
                dgvKhachHang.Columns["MaKH"].Visible = true;
                dgvKhachHang.Columns["MaKH"].HeaderText = "Mã KH";
                dgvKhachHang.Columns["MaKH"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvKhachHang.Columns.Contains("TenKH"))
                dgvKhachHang.Columns["TenKH"].HeaderText = "Tên khách hàng";

            if (dgvKhachHang.Columns.Contains("SDT"))
                dgvKhachHang.Columns["SDT"].HeaderText = "Số điện thoại";

            // Keep TongChiTieu hidden (still used to compute rank)
            if (dgvKhachHang.Columns.Contains("TongChiTieu"))
                dgvKhachHang.Columns["TongChiTieu"].Visible = false;

            // VNĐ formatting for total spent
            TableStyleHelper.ApplyVndFormatting(dgvKhachHang, "TongChiTieu");

            // Ensure only summary columns are visible
            foreach (DataGridViewColumn col in dgvKhachHang.Columns)
            {
                if (col.Name == "TenKH" || col.Name == "SDT" || col.Name == "TongChiTieu" || col.Name == "MaKH")
                    continue;
                col.Visible = false;
            }
        }

        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex < 0) return;

            try
            {
                dgvKhachHang.CurrentCell = dgvKhachHang.Rows[e.RowIndex].Cells[e.ColumnIndex];
                dgvKhachHang.Rows[e.RowIndex].Selected = true;
            }
            catch { }

            DataGridViewRow r = dgvKhachHang.Rows[e.RowIndex];
            selectedCustomer = new KhachHang
            {
                MaKH = Convert.ToInt32(r.Cells["MaKH"].Value),
                TenKH = r.Cells["TenKH"].Value.ToString(),
                SDT = r.Cells["SDT"].Value.ToString()
            };

            selectedCustomerTongChiTieu = 0m;
            if (r.Cells["TongChiTieu"].Value != null && r.Cells["TongChiTieu"].Value != DBNull.Value)
            {
                decimal.TryParse(r.Cells["TongChiTieu"].Value.ToString(), out selectedCustomerTongChiTieu);
            }

            var rankInfo = GetRankByTotalSpent(selectedCustomerTongChiTieu);
            lblTenKhachHang.Text = $"Loại khách: {selectedCustomer.TenKH} - {rankInfo.Rank} ({TableStyleHelper.FormatVnd(selectedCustomerTongChiTieu)})";
        }

        private void btnTimKH_Click(object sender, EventArgs e)
        {
            string keyword = txtSDTKhachHang.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("Vui lòng nhập SĐT hoặc tên khách hàng",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // LẤY TOÀN BỘ KHÁCH HÀNG
                DataTable dt = khachHangBLL.HienThiDanhSachKhachHangTomTat();

                // LỌC AN TOÀN (KHÔNG CRASH – KHÔNG NULL)
                var result = dt.AsEnumerable()
                    .Where(r =>
                        (!r.IsNull("SDT") &&
                         r.Field<string>("SDT").Contains(keyword)) ||
                        (!r.IsNull("TenKH") &&
                         r.Field<string>("TenKH")
                             .IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    );

                if (!result.Any())
                {
                    MessageBox.Show("Không tìm thấy khách hàng",
                                    "Thông báo",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }

                dgvKhachHang.DataSource = result.CopyToDataTable();
                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvKhachHang.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                if (dgvKhachHang.Columns.Contains("MaKH")) dgvKhachHang.Columns["MaKH"].Visible = false;
                if (dgvKhachHang.Columns.Contains("TenKH")) dgvKhachHang.Columns["TenKH"].HeaderText = "Tên";
                if (dgvKhachHang.Columns.Contains("SDT")) dgvKhachHang.Columns["SDT"].HeaderText = "SĐT";
                if (dgvKhachHang.Columns.Contains("SoLanMua")) dgvKhachHang.Columns["SoLanMua"].HeaderText = "Số lần mua";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm khách hàng: " + ex.Message,
                                "Lỗi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        private void btnXoaKH_Click(object sender, EventArgs e)
        {
            selectedCustomer = null;
            selectedCustomerTongChiTieu = 0m;
            lblTenKhachHang.Text = "Loại khách: Khách lẻ";
        }

        // ================= HÓA ĐƠN =================
        private void LoadHoaDon()
        {
            dgvHoaDon.DataSource = donHangBLL.LayDanhSachDonHang();
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Ensure monetary columns display with VNĐ
            TableStyleHelper.ApplyVndFormatting(dgvHoaDon, "TongTien");
        }

        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            selectedMaDH = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells["MaDH"].Value);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (selectedMaDH == null) return;
            InHoaDon(selectedMaDH.Value);
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (selectedMaDH == null) return;
            ExportHoaDonToPdf(selectedMaDH.Value);
        }

        private void InHoaDon(int maDH)
        {
            MessageBox.Show("In hóa đơn mã: " + maDH);
        }

        private void ExportHoaDonToPdf(int maDH)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF|*.pdf";
            sfd.FileName = "HoaDon_" + maDH + ".pdf";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
            Document doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, fs);

            doc.Open();
            doc.Add(new Paragraph("HÓA ĐƠN #" + maDH));
            doc.Close();
            fs.Close();

            MessageBox.Show("Xuất PDF thành công!");
        }
        private void LoadKhachHangToGrid()
        {
            try
            {
                DataTable dt = khachHangBLL.HienThiDanhSachKhachHang();
                dgvKhachHang.DataSource = dt;
                dgvKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (dgvKhachHang.Columns.Contains("MaKH"))
                    dgvKhachHang.Columns["MaKH"].HeaderText = "Mã KH";
                if (dgvKhachHang.Columns.Contains("TenKH"))
                    dgvKhachHang.Columns["TenKH"].HeaderText = "Tên KH";
                if (dgvKhachHang.Columns.Contains("SDT"))
                    dgvKhachHang.Columns["SDT"].HeaderText = "SĐT";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load khách hàng: " + ex.Message);
            }
        }

    }
}
