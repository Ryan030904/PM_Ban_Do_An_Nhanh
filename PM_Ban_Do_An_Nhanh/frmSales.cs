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
        private DanhMucBLL danhMucBLL = new DanhMucBLL();

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


        private ComboBox cboPaymentMethod;

        private string currentMenuSearch = "";

        private ComboBox cboMenuType;
        private string currentMenuType = "Tất cả";


        private DataTable hoaDonTableRaw;
        private string currentHoaDonSearch = "";
        private TextBox txtHistorySearch;
        private Button btnDeleteHoaDon;
        private ComboBox cboHistoryRange;
        private string currentHoaDonRange = "All";

        private const string HoaDonRealIdColumn = "MaDH_Real";

        private TextBox txtCartCustomerTen;
        private TextBox txtCartCustomerSdt;


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
            if (tongChiTieu >= 12000000m) return ("VIP", 0.20m);
            if (tongChiTieu >= 8000000m) return ("Kim Cương", 0.15m);
            if (tongChiTieu >= 5000000m) return ("Bạch Kim", 0.12m);
            if (tongChiTieu >= 2000000m) return ("Vàng", 0.08m);
            if (tongChiTieu >= 500000m) return ("Bạc", 0.05m);
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

            try
            {
                if (mainTabControl != null && tabCustomer != null && mainTabControl.TabPages.Contains(tabCustomer))
                    mainTabControl.TabPages.Remove(tabCustomer);
            }
            catch { }

            EnsureEqualMainTabs();

            if (pnlMonAn != null)
            {
                pnlMonAn.FlowDirection = FlowDirection.TopDown;
                pnlMonAn.WrapContents = false;
            }

            EnsureOrderTabLayout();
            EnsureHistoryTabLayout();
        }

        private static string NormalizeForSearch(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            try
            {
                string s = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(s.Length);
                foreach (char c in s)
                {
                    var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc != UnicodeCategory.NonSpacingMark)
                        sb.Append(c);
                }
                return sb.ToString().Normalize(NormalizationForm.FormC);
            }
            catch
            {
                return input.Trim().ToLowerInvariant();
            }
        }

        private static bool IsDrinkItem(string nameOrCategory)
        {
            string s = NormalizeForSearch(nameOrCategory);
            if (string.IsNullOrWhiteSpace(s)) return false;

            // Heuristic keywords for beverage categories/items
            return s.Contains("nuoc") || s.Contains("uongs") || s.Contains("uong") || s.Contains("giai khat") ||
                   s.Contains("drink") || s.Contains("beverage") ||
                   s.Contains("coca") || s.Contains("pepsi") || s.Contains("sprite") || s.Contains("tra") ||
                   s.Contains("soda") || s.Contains("nuoc ngot");
        }

        private void EnsureMenuTypeComboItems(DataTable monAnTable = null)
        {
            if (cboMenuType == null) return;

            var items = new List<string>();
            items.Add("Tất cả");

            try
            {
                DataTable dm = danhMucBLL.LayDanhSachDanhMuc();
                if (dm != null && dm.Columns.Contains("TenDM"))
                {
                    foreach (DataRow r in dm.Rows)
                    {
                        string name = r["TenDM"]?.ToString();
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        items.Add(name.Trim());
                    }
                }
            }
            catch
            {
            }

            if (items.Count <= 1)
            {
                try
                {
                    if (monAnTable != null && monAnTable.Columns.Contains("TenDM"))
                    {
                        foreach (var name in monAnTable.AsEnumerable()
                                     .Select(r => r["TenDM"] == DBNull.Value ? "" : (r["TenDM"]?.ToString() ?? ""))
                                     .Where(s => !string.IsNullOrWhiteSpace(s))
                                     .Select(s => s.Trim())
                                     .Distinct(StringComparer.OrdinalIgnoreCase))
                        {
                            items.Add(name);
                        }
                    }
                }
                catch
                {
                }
            }

            try
            {
                cboMenuType.BeginUpdate();
                cboMenuType.Items.Clear();
                cboMenuType.Items.AddRange(items.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            }
            finally
            {
                try { cboMenuType.EndUpdate(); } catch { }
            }

            try
            {
                string desired = currentMenuType;
                if (string.Equals(desired, "Đồ ăn", StringComparison.OrdinalIgnoreCase)) desired = "Tất cả";
                if (string.Equals(desired, "Nước uống", StringComparison.OrdinalIgnoreCase)) desired = "Đồ uống";

                if (cboMenuType.Items.Cast<object>().Any(x => string.Equals(x?.ToString(), desired, StringComparison.OrdinalIgnoreCase)))
                    cboMenuType.SelectedItem = desired;
                else
                    cboMenuType.SelectedItem = "Tất cả";
            }
            catch
            {
                try { cboMenuType.SelectedItem = "Tất cả"; } catch { }
            }
        }

        private string GetSelectedMenuType()
        {
            try
            {
                if (cboMenuType == null) return "Tất cả";
                string v = cboMenuType.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(v)) return "Tất cả";
                return v.Trim();
            }
            catch
            {
                return "Tất cả";
            }
        }

        private void ApplyMenuFilters(string keyword, string menuType)
        {
            if (pnlMonAn == null) return;

            currentMenuSearch = keyword ?? "";
            currentMenuType = string.IsNullOrWhiteSpace(menuType) ? "Tất cả" : menuType;

            string needle = NormalizeForSearch(currentMenuSearch);
            string type = currentMenuType;
            if (string.Equals(type, "Nước uống", StringComparison.OrdinalIgnoreCase)) type = "Đồ uống";
            if (string.Equals(type, "Đồ ăn", StringComparison.OrdinalIgnoreCase)) type = "Tất cả";
            string typeNeedle = NormalizeForSearch(type);

            bool anyCard = false;
            int visibleCards = 0;

            foreach (Control c in pnlMonAn.Controls)
            {
                if (c is MenuItemCard card)
                {
                    anyCard = true;

                    string hay = NormalizeForSearch(card.TenMon ?? "");
                    bool okName = string.IsNullOrWhiteSpace(needle) || hay.Contains(needle);

                    string category = card.Tag?.ToString() ?? "";
                    bool okType = true;
                    if (!string.IsNullOrWhiteSpace(typeNeedle) && !string.Equals(typeNeedle, NormalizeForSearch("Tất cả"), StringComparison.OrdinalIgnoreCase))
                    {
                        string catNeedle = NormalizeForSearch(category);
                        okType = string.Equals(catNeedle, typeNeedle, StringComparison.OrdinalIgnoreCase);
                    }

                    bool ok = okName && okType;
                    card.Visible = ok;
                    if (ok) visibleCards++;
                }
                else
                {
                    // keep only labels (e.g. placeholder) when no filter is applied
                    c.Visible = string.IsNullOrWhiteSpace(needle) && string.Equals(type, "Tất cả", StringComparison.OrdinalIgnoreCase);
                }
            }

            if (anyCard && (!string.IsNullOrWhiteSpace(needle) || !string.Equals(type, "Tất cả", StringComparison.OrdinalIgnoreCase)) && visibleCards == 0)
            {
                if (!pnlMonAn.Controls.OfType<Label>().Any(l => string.Equals(l.Name, "lblNoSearchResults", StringComparison.OrdinalIgnoreCase)))
                {
                    var lbl = new Label
                    {
                        Name = "lblNoSearchResults",
                        Text = "Không tìm thấy món phù hợp",
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Margin = new Padding(6)
                    };
                    pnlMonAn.Controls.Add(lbl);
                }
            }
            else
            {
                var toRemove = pnlMonAn.Controls.OfType<Label>()
                    .Where(l => string.Equals(l.Name, "lblNoSearchResults", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var l in toRemove) pnlMonAn.Controls.Remove(l);
            }

            try { pnlMonAn.PerformLayout(); } catch { }
        }

        private void TxtSearchMenu_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyMenuFilters(txtSearchMenu?.Text, GetSelectedMenuType());
            }
            catch { }
        }

        private void CboMenuType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyMenuFilters(txtSearchMenu?.Text, GetSelectedMenuType());
            }
            catch { }
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
                Height = 180,
                Padding = new Padding(8)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var info = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            lblTenKhachHang.AutoSize = true;
            lblTongTien.AutoSize = true;
            lblTenKhachHang.Visible = false;
            info.Controls.Add(lblTongTien);

            var payRow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 0)
            };

            var lblPay = new Label
            {
                Text = "Thanh toán:",
                AutoSize = true,
                Margin = new Padding(0, 6, 6, 0)
            };

            cboPaymentMethod = new NoScrollComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180
            };
            cboPaymentMethod.Items.AddRange(new object[] { "Tiền mặt", "Mã QR" });
            cboPaymentMethod.SelectedIndex = 0;

            payRow.Controls.Add(lblPay);
            payRow.Controls.Add(cboPaymentMethod);
            info.Controls.Add(payRow);

            var customerInput = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 6, 0, 0)
            };
            customerInput.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            customerInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var lblTen = new Label
            {
                Text = "Tên khách:",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };
            txtCartCustomerTen = new TextBox
            {
                Width = 220
            };
            txtCartCustomerTen.Leave += (s, e) =>
            {
                try
                {
                    string raw = txtCartCustomerTen.Text;
                    string normalized = khachHangBLL.ChuanHoaTenKhachHang(raw);
                    if (!string.Equals(raw ?? "", normalized ?? "", StringComparison.Ordinal))
                        txtCartCustomerTen.Text = normalized;
                }
                catch { }
            };

            var lblSdt = new Label
            {
                Text = "SĐT :",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };
            txtCartCustomerSdt = new TextBox
            {
                Width = 220
            };

            customerInput.Controls.Add(lblTen, 0, 0);
            customerInput.Controls.Add(txtCartCustomerTen, 1, 0);
            customerInput.Controls.Add(lblSdt, 0, 1);
            customerInput.Controls.Add(txtCartCustomerSdt, 1, 1);
            info.Controls.Add(customerInput);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = false,
                AutoSize = true,
                Margin = new Padding(0)
            };
            actions.Controls.Add(btnThanhToan);
            actions.Controls.Add(btnHuyDon);
            actions.Controls.Add(btnEditOrder);
            actions.Controls.Add(btnMinusItem);
            actions.Controls.Add(btnPlusItem);

            grid.Controls.Add(info, 0, 0);
            grid.Controls.Add(actions, 1, 0);
            bottom.Controls.Add(grid);
            cartPanel.Controls.Add(bottom);
        }

        private string GetSelectedPaymentMethod()
        {
            try
            {
                if (cboPaymentMethod == null) return "Tiền mặt";
                string v = cboPaymentMethod.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(v)) return "Tiền mặt";
                return v.Trim();
            }
            catch
            {
                return "Tiền mặt";
            }
        }

        private System.Drawing.Image TryLoadQrImage()
        {
            try
            {
                string[] fileNames = new[]
                {
                    "qr.png",
                    "qr.jpg",
                    "qr.jpeg",
                    "qrcode.png",
                    "qrcode.jpg",
                    "default.jpg"
                };

                var roots = new List<string>();
                void AddRoot(string p)
                {
                    if (string.IsNullOrWhiteSpace(p)) return;
                    try
                    {
                        string full = Path.GetFullPath(p);
                        if (!roots.Contains(full, StringComparer.OrdinalIgnoreCase))
                            roots.Add(full);
                    }
                    catch { }
                }

                AddRoot(AppDomain.CurrentDomain.BaseDirectory);
                AddRoot(Application.StartupPath);
                try
                {
                    AddRoot(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                }
                catch { }

                // Probe parent folders to support running from bin\\Debug/Release
                try
                {
                    string cur = AppDomain.CurrentDomain.BaseDirectory;
                    for (int i = 0; i < 6; i++)
                    {
                        var parent = Directory.GetParent(cur);
                        if (parent == null) break;
                        cur = parent.FullName;
                        AddRoot(cur);
                    }
                }
                catch { }

                foreach (var root in roots)
                {
                    foreach (var sub in new[] { "", "Images" })
                    {
                        foreach (var name in fileNames)
                        {
                            string p = string.IsNullOrEmpty(sub) ? Path.Combine(root, name) : Path.Combine(root, sub, name);
                            if (!File.Exists(p)) continue;
                            using (var src = System.Drawing.Image.FromFile(p))
                            {
                                return new Bitmap(src);
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private DialogResult ShowQrPaymentDialog(decimal amount)
        {
            using (var f = new Form())
            {
                f.Text = "Thanh toán mã QR";
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ClientSize = new Size(520, 640);
                f.BackColor = Color.White;
                f.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(18)
                };
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var lblAmount = new Label
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    Text = "Số tiền: " + TableStyleHelper.FormatVnd(amount),
                    Margin = new Padding(0, 0, 0, 6)
                };

                var lblHint = new Label
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                    ForeColor = Color.FromArgb(73, 80, 87),
                    Text = "Vui lòng quét mã để thanh toán",
                    Margin = new Padding(0, 0, 0, 12)
                };

                var qrHost = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(18),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.White,
                    Margin = new Padding(0)
                };

                var img = TryLoadQrImage();
                pb.Image = img;

                qrHost.Controls.Add(pb);

                var btnRow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    AutoSize = true,
                    Padding = new Padding(0, 14, 0, 0)
                };

                var btnOk = new Button
                {
                    Text = "Đã thanh toán",
                    AutoSize = false,
                    Size = new Size(140, 40),
                    BackColor = Color.FromArgb(0, 123, 255),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) => { f.DialogResult = DialogResult.OK; f.Close(); };

                var btnCancel = new Button
                {
                    Text = "Hủy",
                    AutoSize = false,
                    Size = new Size(110, 40),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

                btnRow.Controls.Add(btnOk);
                btnRow.Controls.Add(btnCancel);

                layout.Controls.Add(lblAmount, 0, 0);
                layout.Controls.Add(lblHint, 0, 1);
                layout.Controls.Add(qrHost, 0, 2);
                layout.Controls.Add(btnRow, 0, 3);
                f.Controls.Add(layout);

                f.AcceptButton = btnOk;
                f.CancelButton = btnCancel;

                f.FormClosed += (s, e) =>
                {
                    try
                    {
                        if (pb.Image != null)
                        {
                            var old = pb.Image;
                            pb.Image = null;
                            old.Dispose();
                        }
                    }
                    catch { }
                };

                return f.ShowDialog(this);
            }
        }

        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            MoveCartPanelToSelectedTab();

            try
            {
                if (!string.IsNullOrWhiteSpace(currentMenuSearch))
                    txtSearchMenu.Text = currentMenuSearch;

                if (cboMenuType != null)
                    cboMenuType.SelectedItem = currentMenuType;
            }
            catch { }
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

            btnRemoveItem.Visible = false;

            btnRemoveItem.BackColor = Color.FromArgb(220, 53, 69);
            btnRemoveItem.ForeColor = Color.White;
            btnRemoveItem.FlatStyle = FlatStyle.Flat;
            btnRemoveItem.FlatAppearance.BorderSize = 0;

            btnHuyDon.BackColor = Color.FromArgb(255, 193, 7);
            btnHuyDon.ForeColor = Color.Black;
            btnHuyDon.FlatStyle = FlatStyle.Flat;
            btnHuyDon.FlatAppearance.BorderSize = 0;

            btnEditOrder.BackColor = Color.FromArgb(108, 117, 125);
            btnEditOrder.ForeColor = Color.White;
            btnEditOrder.FlatStyle = FlatStyle.Flat;
            btnEditOrder.FlatAppearance.BorderSize = 0;

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

            btnEditOrder.AutoSize = true;
            btnEditOrder.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Keep the action row stable when switching Sửa <-> Xác nhận (avoid clipping/overlap)
            btnEditOrder.AutoSize = false;
            btnHuyDon.AutoSize = false;
            btnThanhToan.AutoSize = false;

            btnEditOrder.Size = new Size(100, 34);
            btnHuyDon.Size = new Size(90, 34);
            btnThanhToan.Size = new Size(100, 34);

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
            const int preferredMinRight = 520;
            const int preferredRight = 620;
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

            var searchBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(8, 8, 8, 8)
            };
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            searchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblSearch = new Label
            {
                Text = "Tìm món:",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 8, 0)
            };

            txtSearchMenu.Dock = DockStyle.Fill;
            txtSearchMenu.Margin = new Padding(0, 2, 0, 0);
            txtSearchMenu.TextChanged -= TxtSearchMenu_TextChanged;
            txtSearchMenu.TextChanged += TxtSearchMenu_TextChanged;

            var lblType = new Label
            {
                Text = "Loại:",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(12, 6, 8, 0)
            };

            cboMenuType = new NoScrollComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Arial", 10F)
            };
            cboMenuType.SelectedIndexChanged -= CboMenuType_SelectedIndexChanged;
            cboMenuType.SelectedIndexChanged += CboMenuType_SelectedIndexChanged;
            EnsureMenuTypeComboItems();

            searchBar.Controls.Add(lblSearch, 0, 0);
            searchBar.Controls.Add(txtSearchMenu, 1, 0);
            searchBar.Controls.Add(lblType, 2, 0);
            searchBar.Controls.Add(cboMenuType, 3, 0);

            pnlMonAn.Dock = DockStyle.Fill;
            left.Controls.Add(pnlMonAn);
            left.Controls.Add(searchBar);
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

            try
            {
                bool already = tabHistory.Controls.OfType<FlowLayoutPanel>().Any() && tabHistory.Controls.Contains(dgvHoaDon);
                if (already) return;
            }
            catch { }

            if (txtHistorySearch == null)
            {
                txtHistorySearch = new TextBox
                {
                    Width = 260,
                    Font = new System.Drawing.Font("Arial", 10F)
                };
                txtHistorySearch.TextChanged += (s, e) => ApplyHoaDonFilters(txtHistorySearch.Text);
            }

            if (btnDeleteHoaDon == null)
            {
                btnDeleteHoaDon = new Button
                {
                    Text = "Xóa",
                    AutoSize = true
                };
                btnDeleteHoaDon.Click += new EventHandler(btnDeleteHoaDon_Click);
            }

            if (cboHistoryRange == null)
            {
                cboHistoryRange = new NoScrollComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 170,
                    Font = new System.Drawing.Font("Arial", 10F)
                };
                cboHistoryRange.Items.AddRange(new object[] { "Tất cả", "1 ngày qua", "1 tuần qua", "1 tháng qua", "1 năm qua" });
                cboHistoryRange.SelectedIndex = 0;
                currentHoaDonRange = "All";
                cboHistoryRange.SelectedIndexChanged += (s, e) =>
                {
                    currentHoaDonRange = GetHoaDonRangeMode();
                    ApplyHoaDonFilters(txtHistorySearch?.Text ?? currentHoaDonSearch);
                };
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
            top.Controls.Add(new Label { Text = "Thời gian:", AutoSize = true, Margin = new Padding(16, 6, 6, 0) });
            top.Controls.Add(cboHistoryRange);
            top.Controls.Add(new Label { Text = "Tìm:", AutoSize = true, Margin = new Padding(16, 6, 6, 0) });
            top.Controls.Add(txtHistorySearch);
            top.Controls.Add(btnDeleteHoaDon);

            dgvHoaDon.Dock = DockStyle.Fill;
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHoaDon.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            tabHistory.Controls.Add(dgvHoaDon);
            tabHistory.Controls.Add(top);
        }

        private string GetHoaDonRangeMode()
        {
            try
            {
                if (cboHistoryRange == null) return currentHoaDonRange;
                string t = cboHistoryRange.SelectedItem?.ToString() ?? "";
                if (string.Equals(t, "1 ngày qua", StringComparison.OrdinalIgnoreCase)) return "1d";
                if (string.Equals(t, "1 tuần qua", StringComparison.OrdinalIgnoreCase)) return "1w";
                if (string.Equals(t, "1 tháng qua", StringComparison.OrdinalIgnoreCase)) return "1m";
                if (string.Equals(t, "1 năm qua", StringComparison.OrdinalIgnoreCase)) return "1y";
                return "All";
            }
            catch
            {
                return currentHoaDonRange;
            }
        }

        private DateTime? GetHoaDonRangeCutoff(string mode)
        {
            try
            {
                var now = DateTime.Now;
                if (string.Equals(mode, "1d", StringComparison.OrdinalIgnoreCase)) return now.AddDays(-1);
                if (string.Equals(mode, "1w", StringComparison.OrdinalIgnoreCase)) return now.AddDays(-7);
                if (string.Equals(mode, "1m", StringComparison.OrdinalIgnoreCase)) return now.AddMonths(-1);
                if (string.Equals(mode, "1y", StringComparison.OrdinalIgnoreCase)) return now.AddYears(-1);
                return null;
            }
            catch
            {
                return null;
            }
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

            try
            {
                if (!string.IsNullOrWhiteSpace(currentMenuSearch) || !string.Equals(currentMenuType, "Tất cả", StringComparison.OrdinalIgnoreCase))
                    ApplyMenuFilters(currentMenuSearch, currentMenuType);
            }
            catch { }
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
                                    card.Tag = IsDrinkItem(friendlyName) ? "Nước uống" : "Đồ ăn";
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

                try { EnsureMenuTypeComboItems(dt); } catch { }

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

                        try
                        {
                            if (r.Table.Columns.Contains("TenDM"))
                                card.Tag = r["TenDM"]?.ToString() ?? "";
                        }
                        catch { }

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

            try
            {
                if (!string.IsNullOrWhiteSpace(currentMenuSearch) || !string.Equals(currentMenuType, "Tất cả", StringComparison.OrdinalIgnoreCase))
                    ApplyMenuFilters(currentMenuSearch, currentMenuType);
            }
            catch { }
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

            try
            {
                if (dgvOrderList.Columns.Contains("TenMon"))
                {
                    dgvOrderList.Columns["TenMon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvOrderList.Columns["TenMon"].FillWeight = 58F;
                    dgvOrderList.Columns["TenMon"].MinimumWidth = 260;
                }

                if (dgvOrderList.Columns.Contains("SoLuong"))
                {
                    dgvOrderList.Columns["SoLuong"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvOrderList.Columns["SoLuong"].FillWeight = 10F;
                    dgvOrderList.Columns["SoLuong"].MinimumWidth = 60;
                    dgvOrderList.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (dgvOrderList.Columns.Contains("DonGia"))
                {
                    dgvOrderList.Columns["DonGia"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvOrderList.Columns["DonGia"].FillWeight = 16F;
                    dgvOrderList.Columns["DonGia"].MinimumWidth = 110;
                }

                if (dgvOrderList.Columns.Contains("ThanhTien"))
                {
                    dgvOrderList.Columns["ThanhTien"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvOrderList.Columns["ThanhTien"].FillWeight = 16F;
                    dgvOrderList.Columns["ThanhTien"].MinimumWidth = 120;
                }
            }
            catch { }

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
            try
            {
                if (txtCartCustomerTen != null) txtCartCustomerTen.Text = "";
                if (txtCartCustomerSdt != null) txtCartCustomerSdt.Text = "";
            }
            catch { }
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

                try
                {
                    string tenNhap = txtCartCustomerTen?.Text?.Trim() ?? "";
                    string sdtNhap = txtCartCustomerSdt?.Text?.Trim() ?? "";

                    try { tenNhap = khachHangBLL.ChuanHoaTenKhachHang(tenNhap); } catch { }

                    if (!string.IsNullOrWhiteSpace(sdtNhap))
                    {
                        KhachHang kh = null;
                        try { kh = khachHangBLL.LayThongTinKhachHangBySDT(sdtNhap); } catch { }

                        if (kh == null)
                        {
                            string tenToSave = string.IsNullOrWhiteSpace(tenNhap) ? "Khách" : tenNhap;
                            try
                            {
                                khachHangBLL.ThemKhachHang(tenToSave, sdtNhap, "");
                                kh = khachHangBLL.LayThongTinKhachHangBySDT(sdtNhap);
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                if (string.Equals(kh.TrangThai ?? "", "Inactive", StringComparison.OrdinalIgnoreCase))
                                {
                                    khachHangBLL.CapNhatTrangThaiKhachHang(sdtNhap, "Active");
                                    kh.TrangThai = "Active";
                                }
                            }
                            catch { }

                            if (!string.IsNullOrWhiteSpace(tenNhap) && !string.Equals(kh.TenKH ?? "", tenNhap, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    khachHangBLL.CapNhatKhachHang(tenNhap, sdtNhap, kh.DiaChi ?? "");
                                    kh.TenKH = tenNhap;
                                }
                                catch { }
                            }
                        }

                        selectedCustomer = kh;
                        selectedCustomerTongChiTieu = 0m;
                        try
                        {
                            DataTable sum = khachHangBLL.HienThiDanhSachKhachHangTomTat();
                            if (sum != null && sum.Columns.Contains("SDT") && sum.Columns.Contains("TongChiTieu"))
                            {
                                var r = sum.AsEnumerable().FirstOrDefault(x => string.Equals(x["SDT"]?.ToString() ?? "", sdtNhap, StringComparison.OrdinalIgnoreCase));
                                if (r != null && r["TongChiTieu"] != DBNull.Value)
                                    decimal.TryParse(r["TongChiTieu"].ToString(), out selectedCustomerTongChiTieu);
                            }
                        }
                        catch { }

                        if (selectedCustomer != null)
                        {
                            var rankInfo0 = GetRankByTotalSpent(selectedCustomerTongChiTieu);
                            lblTenKhachHang.Text = $"Loại khách: {selectedCustomer.TenKH} - {rankInfo0.Rank} ({TableStyleHelper.FormatVnd(selectedCustomerTongChiTieu)})";
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(tenNhap))
                    {
                        // Lookup by name when phone is not provided.
                        try
                        {
                            var byName = khachHangBLL.TimKhachHangTheoTen(tenNhap, "All");
                            if (byName != null)
                            {
                                // Prefer active customers if status exists
                                DataRow[] activeRows = null;
                                try
                                {
                                    if (byName.Columns.Contains("TrangThai"))
                                        activeRows = byName.Select("TrangThai IS NULL OR TrangThai = 'Active'");
                                }
                                catch { activeRows = null; }

                                var rows = (activeRows != null && activeRows.Length > 0) ? activeRows : byName.Select();

                                if (rows != null && rows.Length == 1)
                                {
                                    var r = rows[0];
                                    var kh = new KhachHang
                                    {
                                        MaKH = r["MaKH"] == DBNull.Value ? 0 : Convert.ToInt32(r["MaKH"]),
                                        TenKH = r["TenKH"]?.ToString() ?? tenNhap,
                                        SDT = r.Table.Columns.Contains("SDT") ? (r["SDT"]?.ToString() ?? "") : "",
                                        DiaChi = r.Table.Columns.Contains("DiaChi") ? (r["DiaChi"]?.ToString() ?? "") : "",
                                        TrangThai = r.Table.Columns.Contains("TrangThai") ? (r["TrangThai"]?.ToString()) : null
                                    };

                                    if (!string.IsNullOrWhiteSpace(kh.TrangThai)
                                        && string.Equals(kh.TrangThai, "Inactive", StringComparison.OrdinalIgnoreCase)
                                        && !string.IsNullOrWhiteSpace(kh.SDT))
                                    {
                                        try
                                        {
                                            khachHangBLL.CapNhatTrangThaiKhachHang(kh.SDT, "Active");
                                            kh.TrangThai = "Active";
                                        }
                                        catch { }
                                    }

                                    selectedCustomer = kh;
                                    selectedCustomerTongChiTieu = 0m;
                                    try
                                    {
                                        if (!string.IsNullOrWhiteSpace(kh.SDT))
                                            selectedCustomerTongChiTieu = khachHangBLL.LayTongChiTieuBySDT(kh.SDT);
                                    }
                                    catch { }

                                    var rankInfo0 = GetRankByTotalSpent(selectedCustomerTongChiTieu);
                                    lblTenKhachHang.Text = $"Loại khách: {selectedCustomer.TenKH} - {rankInfo0.Rank} ({TableStyleHelper.FormatVnd(selectedCustomerTongChiTieu)})";
                                }
                                else if (rows != null && rows.Length > 1)
                                {
                                    selectedCustomer = null;
                                    selectedCustomerTongChiTieu = 0m;
                                    lblTenKhachHang.Text = "Loại khách: Khách lẻ";
                                    MessageBox.Show("Tên khách hàng bị trùng. Vui lòng nhập SĐT để đối chiếu đúng khách.");
                                }
                                else
                                {
                                    selectedCustomer = null;
                                    selectedCustomerTongChiTieu = 0m;
                                    lblTenKhachHang.Text = "Loại khách: Khách lẻ";
                                }
                            }
                        }
                        catch
                        {
                            selectedCustomer = null;
                            selectedCustomerTongChiTieu = 0m;
                            lblTenKhachHang.Text = "Loại khách: Khách lẻ";
                        }
                    }
                    else
                    {
                        selectedCustomer = null;
                        selectedCustomerTongChiTieu = 0m;
                        lblTenKhachHang.Text = "Loại khách: Khách lẻ";
                    }
                }
                catch { }

                decimal tongChiTieuTruoc = selectedCustomer != null ? selectedCustomerTongChiTieu : 0m;
                var rankInfo = GetRankByTotalSpent(tongChiTieuTruoc);
                decimal discount = 0m;

                if (selectedCustomer != null && rankInfo.DiscountRate > 0)
                {
                    discount = Math.Round(tongTienGoc * rankInfo.DiscountRate, 0, MidpointRounding.AwayFromZero);
                }

                decimal tongTien = tongTienGoc - discount;

                string paymentMethod = GetSelectedPaymentMethod();
                if (string.Equals(paymentMethod, "Mã QR", StringComparison.OrdinalIgnoreCase))
                {
                    var qrResult = ShowQrPaymentDialog(tongTien);
                    if (qrResult != DialogResult.OK)
                    {
                        return;
                    }
                }

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
            dgvKhachHang.DataSource = khachHangBLL.HienThiDanhSachKhachHangTomTatTheoTrangThai("Active");
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
                SDT = r.Cells["SDT"].Value.ToString(),
                TrangThai = (dgvKhachHang.Columns.Contains("TrangThai") ? (r.Cells["TrangThai"].Value?.ToString() ?? "") : "")
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
                DataTable dt = khachHangBLL.HienThiDanhSachKhachHangTomTatTheoTrangThai("Active");

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

                dgvKhachHang.EnableHeadersVisualStyles = false;
                dgvKhachHang.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

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
            hoaDonTableRaw = donHangBLL.LayDanhSachDonHang();
            ApplyHoaDonFilters(txtHistorySearch?.Text ?? currentHoaDonSearch);
        }

        private void ApplyHoaDonFilters(string keyword)
        {
            currentHoaDonSearch = keyword ?? "";
            if (dgvHoaDon == null) return;

            DataTable dt = hoaDonTableRaw;
            if (dt == null)
            {
                dgvHoaDon.DataSource = null;
                return;
            }

            string needle = NormalizeForSearch(currentHoaDonSearch);

            currentHoaDonRange = GetHoaDonRangeMode();
            DateTime? cutoff = GetHoaDonRangeCutoff(currentHoaDonRange);

            IEnumerable<DataRow> rows = null;

            if (string.IsNullOrWhiteSpace(needle))
            {
                try
                {
                    rows = dt.AsEnumerable()
                        .Where(r =>
                        {
                            if (cutoff == null) return true;
                            try
                            {
                                if (r.Table.Columns.Contains("NgayLap") && r["NgayLap"] != DBNull.Value)
                                {
                                    DateTime d = Convert.ToDateTime(r["NgayLap"]);
                                    return d >= cutoff.Value;
                                }
                            }
                            catch { }
                            return true;
                        })
                        .OrderBy(r =>
                        {
                            try { return Convert.ToInt32(r["MaDH"]); } catch { return int.MaxValue; }
                        });
                }
                catch { rows = null; }
            }
            else
            {
                try
                {
                    rows = dt.AsEnumerable()
                        .Where(r =>
                        {
                            if (cutoff != null)
                            {
                                try
                                {
                                    if (r.Table.Columns.Contains("NgayLap") && r["NgayLap"] != DBNull.Value)
                                    {
                                        DateTime d = Convert.ToDateTime(r["NgayLap"]);
                                        if (d < cutoff.Value) return false;
                                    }
                                }
                                catch { }
                            }

                            string ten = (r.Table.Columns.Contains("TenKH") && !r.IsNull("TenKH")) ? (r["TenKH"]?.ToString() ?? "") : "";
                            string sdt = (r.Table.Columns.Contains("SDT_KhachHang") && !r.IsNull("SDT_KhachHang")) ? (r["SDT_KhachHang"]?.ToString() ?? "") : "";
                            return NormalizeForSearch(ten).Contains(needle) || NormalizeForSearch(sdt).Contains(needle);
                        })
                        .OrderBy(r =>
                        {
                            try { return Convert.ToInt32(r["MaDH"]); } catch { return int.MaxValue; }
                        });
                }
                catch { rows = null; }
            }

            DataTable view = dt.Clone();
            if (rows != null)
            {
                try
                {
                    if (rows.Any()) view = rows.CopyToDataTable();
                }
                catch { }
            }

            if (!view.Columns.Contains(HoaDonRealIdColumn))
                view.Columns.Add(HoaDonRealIdColumn, typeof(int));

            int i = 1;
            foreach (DataRow r in view.Rows)
            {
                int real = 0;
                try { real = Convert.ToInt32(r["MaDH"]); } catch { }
                try { r[HoaDonRealIdColumn] = real; } catch { }
                try { r["MaDH"] = i; } catch { }
                i++;
            }

            dgvHoaDon.DataSource = view;
            dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            try
            {
                if (dgvHoaDon.Columns.Contains(HoaDonRealIdColumn))
                    dgvHoaDon.Columns[HoaDonRealIdColumn].Visible = false;
                if (dgvHoaDon.Columns.Contains("MaDH"))
                    dgvHoaDon.Columns["MaDH"].HeaderText = "MaDH";
            }
            catch { }
            try { TableStyleHelper.ApplyVndFormatting(dgvHoaDon, "TongTien"); } catch { }
        }

        private void btnDeleteHoaDon_Click(object sender, EventArgs e)
        {
            int? ma = selectedMaDH;
            try
            {
                if (ma == null && dgvHoaDon != null && dgvHoaDon.CurrentRow != null)
                {
                    object v;
                    if (dgvHoaDon.Columns.Contains(HoaDonRealIdColumn))
                        v = dgvHoaDon.CurrentRow.Cells[HoaDonRealIdColumn].Value;
                    else
                        v = dgvHoaDon.CurrentRow.Cells["MaDH"].Value;
                    if (v != null && v != DBNull.Value) ma = Convert.ToInt32(v);
                }
            }
            catch { }

            if (ma == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa hóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                bool ok = donHangBLL.XoaDonHang(ma.Value);
                if (!ok)
                {
                    MessageBox.Show("Xóa hóa đơn thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedMaDH = null;
                LoadHoaDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            try
            {
                if (dgvHoaDon.Columns.Contains(HoaDonRealIdColumn))
                    selectedMaDH = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells[HoaDonRealIdColumn].Value);
                else
                    selectedMaDH = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells["MaDH"].Value);
            }
            catch
            {
                selectedMaDH = null;
            }
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
            ShowHoaDonDialog(maDH);
        }

        private void ShowHoaDonDialog(int maDH)
        {
            DataTable dt;
            try
            {
                dt = donHangBLL.LayChiTietDonHangChoIn(maDH);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lấy được dữ liệu hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow first = dt.Rows[0];
            DateTime ngayLap = DateTime.Now;
            decimal tongTien = 0m;
            string tenKhDb = "";
            string sdtKhDb = "";
            string trangThai = "";

            try
            {
                if (first.Table.Columns.Contains("TenKH") && first["TenKH"] != DBNull.Value)
                    tenKhDb = first["TenKH"]?.ToString() ?? "";
            }
            catch { }

            try
            {
                if (first.Table.Columns.Contains("SDT_KhachHang") && first["SDT_KhachHang"] != DBNull.Value)
                    sdtKhDb = first["SDT_KhachHang"]?.ToString() ?? "";
            }
            catch { }

            try
            {
                if (first.Table.Columns.Contains("NgayLap") && first["NgayLap"] != DBNull.Value)
                    ngayLap = Convert.ToDateTime(first["NgayLap"]);
            }
            catch { }

            try
            {
                if (first.Table.Columns.Contains("TongTien") && first["TongTien"] != DBNull.Value)
                    tongTien = Convert.ToDecimal(first["TongTien"]);
            }
            catch { }

            try
            {
                if (first.Table.Columns.Contains("TrangThaiThanhToan"))
                    trangThai = first["TrangThaiThanhToan"]?.ToString() ?? "";
            }
            catch { }

            DataTable itemsTable = new DataTable();
            itemsTable.Columns.Add("TenMon", typeof(string));
            itemsTable.Columns.Add("SoLuong", typeof(int));
            itemsTable.Columns.Add("DonGia", typeof(decimal));
            itemsTable.Columns.Add("ThanhTien", typeof(decimal));

            try
            {
                foreach (DataRow r in dt.Rows)
                {
                    string tenMon = r.Table.Columns.Contains("TenMon") ? (r["TenMon"]?.ToString() ?? "") : "";
                    int soLuong = 0;
                    decimal donGia = 0m;
                    decimal thanhTien = 0m;

                    try { if (r.Table.Columns.Contains("SoLuong") && r["SoLuong"] != DBNull.Value) soLuong = Convert.ToInt32(r["SoLuong"]); } catch { }
                    try { if (r.Table.Columns.Contains("DonGia") && r["DonGia"] != DBNull.Value) donGia = Convert.ToDecimal(r["DonGia"]); } catch { }
                    try { if (r.Table.Columns.Contains("ThanhTien") && r["ThanhTien"] != DBNull.Value) thanhTien = Convert.ToDecimal(r["ThanhTien"]); } catch { }

                    itemsTable.Rows.Add(tenMon, soLuong, donGia, thanhTien);
                }
            }
            catch { }

            using (var f = new Form())
            {
                f.Text = "Hóa đơn";
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MinimizeBox = false;
                f.MaximizeBox = false;

                int rowCount = 0;
                try { rowCount = dt?.Rows?.Count ?? 0; } catch { }
                int height = 320 + Math.Min(12, Math.Max(0, rowCount)) * 26;
                height = Math.Min(720, Math.Max(520, height));
                f.ClientSize = new Size(660, height);

                f.BackColor = Color.FromArgb(245, 245, 245);

                var paper = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(18)
                };

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4
                };
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                bool hasCustomerInfo = !string.IsNullOrWhiteSpace(tenKhDb) || !string.IsNullOrWhiteSpace(sdtKhDb);

                int headerRows = 4;
                if (hasCustomerInfo) headerRows++;
                if (!string.IsNullOrWhiteSpace(sdtKhDb)) headerRows++;

                var header = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    ColumnCount = 1,
                    RowCount = headerRows,
                    AutoSize = true
                };
                header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                var lblTitle = new Label
                {
                    Text = "HÓA ĐƠN",
                    AutoSize = false,
                    Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 44
                };

                var lblMa = new Label
                {
                    Text = "Mã hóa đơn: " + maDH,
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                    Margin = new Padding(0, 2, 0, 2)
                };

                var lblNgay = new Label
                {
                    Text = "Ngày: " + ngayLap.ToString("dd/MM/yyyy HH:mm"),
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                    Margin = new Padding(0, 2, 0, 2)
                };

                var lblStatus = new Label
                {
                    Text = string.IsNullOrWhiteSpace(trangThai) ? "Trạng thái: -" : ("Trạng thái: " + trangThai),
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                    Margin = new Padding(0, 2, 0, 2)
                };

                int row = 0;
                header.Controls.Add(lblTitle, 0, row++);
                header.Controls.Add(lblMa, 0, row++);
                header.Controls.Add(lblNgay, 0, row++);

                if (hasCustomerInfo)
                {
                    string tenDisplay = string.IsNullOrWhiteSpace(tenKhDb) ? "Khách" : tenKhDb;
                    var lblKhach = new Label
                    {
                        Text = "Khách: " + tenDisplay,
                        AutoSize = true,
                        Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                        Margin = new Padding(0, 2, 0, 2)
                    };
                    header.Controls.Add(lblKhach, 0, row++);

                    if (!string.IsNullOrWhiteSpace(sdtKhDb))
                    {
                        var lblSdt = new Label
                        {
                            Text = "SĐT: " + sdtKhDb,
                            AutoSize = true,
                            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular),
                            Margin = new Padding(0, 2, 0, 2)
                        };
                        header.Controls.Add(lblSdt, 0, row++);
                    }
                }

                header.Controls.Add(lblStatus, 0, row++);

                var separatorTop = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 1,
                    BackColor = Color.FromArgb(230, 230, 230),
                    Margin = new Padding(0, 10, 0, 10)
                };

                var dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AllowUserToResizeColumns = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle = DataGridViewCellBorderStyle.None,
                    ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                    EnableHeadersVisualStyles = false,
                    GridColor = Color.White,
                    MultiSelect = false
                };
                dgv.DataSource = itemsTable;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
                dgv.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10F);
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 235, 235);
                dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
                dgv.RowTemplate.Height = 28;
                dgv.ColumnHeadersHeight = 34;
                dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                if (dgv.Columns.Contains("TenMon")) dgv.Columns["TenMon"].HeaderText = "Tên món";
                if (dgv.Columns.Contains("SoLuong")) dgv.Columns["SoLuong"].HeaderText = "SL";
                if (dgv.Columns.Contains("DonGia")) dgv.Columns["DonGia"].HeaderText = "Đơn giá";
                if (dgv.Columns.Contains("ThanhTien")) dgv.Columns["ThanhTien"].HeaderText = "Thành tiền";

                if (dgv.Columns.Contains("SoLuong")) dgv.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (dgv.Columns.Contains("DonGia")) dgv.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                if (dgv.Columns.Contains("ThanhTien")) dgv.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                try
                {
                    if (dgv.Columns.Contains("TenMon"))
                    {
                        dgv.Columns["TenMon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgv.Columns["TenMon"].FillWeight = 60;
                        dgv.Columns["TenMon"].MinimumWidth = 320;
                        dgv.Columns["TenMon"].ToolTipText = "Tên món";
                    }
                    if (dgv.Columns.Contains("SoLuong"))
                    {
                        dgv.Columns["SoLuong"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgv.Columns["SoLuong"].FillWeight = 10;
                        dgv.Columns["SoLuong"].MinimumWidth = 70;
                    }
                    if (dgv.Columns.Contains("DonGia"))
                    {
                        dgv.Columns["DonGia"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgv.Columns["DonGia"].FillWeight = 15;
                        dgv.Columns["DonGia"].MinimumWidth = 120;
                    }
                    if (dgv.Columns.Contains("ThanhTien"))
                    {
                        dgv.Columns["ThanhTien"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgv.Columns["ThanhTien"].FillWeight = 15;
                        dgv.Columns["ThanhTien"].MinimumWidth = 140;
                    }

                    dgv.CellFormatting += (s, e) =>
                    {
                        try
                        {
                            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                            var col = dgv.Columns[e.ColumnIndex];
                            if (col == null) return;
                            if (!string.Equals(col.Name, "TenMon", StringComparison.OrdinalIgnoreCase)) return;
                            var v = e.Value?.ToString() ?? "";
                            dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = v;
                        }
                        catch { }
                    };
                }
                catch { }

                try
                {
                    TableStyleHelper.ApplyVndFormatting(dgv, "DonGia", "ThanhTien");
                }
                catch { }

                var bottom = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    AutoSize = true
                };

                var btnClose = new Button
                {
                    Text = "Đóng",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                    Padding = new Padding(18, 10, 18, 10),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => f.Close();

                var lblTotal = new Label
                {
                    AutoSize = true,
                    Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold),
                    Text = "Tổng tiền: " + TableStyleHelper.FormatVnd(tongTien),
                    Margin = new Padding(0, 8, 16, 0)
                };

                bottom.Controls.Add(btnClose);
                bottom.Controls.Add(lblTotal);

                root.Controls.Add(header, 0, 0);
                root.Controls.Add(separatorTop, 0, 1);
                root.Controls.Add(dgv, 0, 2);
                root.Controls.Add(bottom, 0, 3);

                paper.Controls.Add(root);
                f.Controls.Add(paper);

                f.ShowDialog(this);
            }
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

                dgvKhachHang.EnableHeadersVisualStyles = false;
                dgvKhachHang.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

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

        private void pnlMonAn_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
