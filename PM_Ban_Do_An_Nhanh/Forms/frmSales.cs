using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Controls;
using PM_Ban_Do_An_Nhanh.Helpers;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh.Forms
{
    public class frmSales : Form
    {
        private FlowLayoutPanel flowLayoutPanelItems;
        private CartControl cartControl;
        private MonAnBLL monAnBLL = new MonAnBLL();
        private DanhMucBLL danhMucBLL = new DanhMucBLL();
        private Button btnRefresh;

        public frmSales()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Bán hàng";
            this.ClientSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            flowLayoutPanelItems = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(12),
                BackColor = Color.White
            };

            cartControl = new CartControl
            {
                Dock = DockStyle.Right,
                Width = 360
            };
            cartControl.PlaceOrderClicked += CartControl_PlaceOrderClicked;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Dock = DockStyle.Top,
                Height = 36
            };
            btnRefresh.Click += (s, e) => LoadItems();

            // layout: top refresh, main flow panel and cart docked right
            this.Controls.Add(flowLayoutPanelItems);
            this.Controls.Add(cartControl);
            this.Controls.Add(btnRefresh);

            this.Load += (s, e) => LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                flowLayoutPanelItems.Controls.Clear();
                DataTable dt = monAnBLL.HienThiDanhSachMonAn() ?? new DataTable();
                foreach (DataRow r in dt.Rows)
                {
                    CreateItemCard(r);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void CreateItemCard(DataRow itemRow)
        {
            var pnl = new Panel { Width = 240, Height = 180, Margin = new Padding(8), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };

            var pb = new PictureBox
            {
                Size = new Size(180, 100),
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = 8,
                Top = 8,
                Image = ImageHelper.LoadMenuItemImage(itemRow["HinhAnh"]?.ToString() ?? "")
            };

            var lblName = new Label { Text = $"{itemRow["MaMon"]} {itemRow["TenMon"]}", Left = 8, Top = 112, Width = 180, AutoEllipsis = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var lblPrice = new Label { Text = $"{Convert.ToDecimal(itemRow["Gia"]):N0} VND", ForeColor = Color.DarkRed, Left = 8, Top = 132, Width = 120 };

            var nudQty = new NumericUpDown { Left = 140, Top = 128, Width = 56, Minimum = 1, Value = 1 };
            var btnAdd = new Button { Text = "Add", Left = 196, Top = 126, Width = 40, BackColor = Color.FromArgb(37, 150, 84), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

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

        private void CartControl_PlaceOrderClicked(object sender, EventArgs e)
        {
            var dt = cartControl.GetCartTable();
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Gi? hàng tr?ng.");
                return;
            }

            decimal total = 0m;
            int items = 0;
            foreach (DataRow r in dt.Rows)
            {
                total += Convert.ToDecimal(r["Total"]);
                items += Convert.ToInt32(r["Quantity"]);
            }

            // show simple summary. Replace with real order flow.
            var msg = $"Placing order: {items} item(s), {dt.Rows.Count} distinct product(s). Total: {total:N0} VND";
            MessageBox.Show(msg, "Order", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // After successful order, clear cart
            cartControl.Clear();
        }
    }
}