using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.Helpers;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class MenuItemCard : UserControl
    {
        private sealed class NoScrollNumericUpDown : NumericUpDown
        {
            private const int WM_MOUSEWHEEL = 0x020A;

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                if (e is HandledMouseEventArgs h) h.Handled = true;

                Control target = null;
                Control p = Parent;
                while (p != null)
                {
                    if (p is ScrollableControl sc && sc.AutoScroll)
                    {
                        target = p;
                        break;
                    }
                    p = p.Parent;
                }

                if (target != null && target.IsHandleCreated)
                {
                    int keys = ((int)ModifierKeys) & 0xFFFF;
                    int wParam = (e.Delta << 16) | keys;
                    var pos = Cursor.Position;
                    int lParam = (pos.Y << 16) | (pos.X & 0xFFFF);
                    try { SendMessage(target.Handle, WM_MOUSEWHEEL, (IntPtr)wParam, (IntPtr)lParam); } catch { }
                }
            }
        }

        public MenuItemCard()
        {
            InitializeComponent();
            this.btnAdd.Click += BtnAdd_Click;
            this.nudQuantity.Leave += (s, e) => EnsureQuantityDefault();
            this.nudQuantity.Validating += (s, e) => EnsureQuantityDefault();
            this.DoubleBuffered = true;
        }

        private void EnsureQuantityDefault()
        {
            try
            {
                if (nudQuantity == null) return;
                string t = nudQuantity.Text;
                if (string.IsNullOrWhiteSpace(t))
                {
                    nudQuantity.Value = nudQuantity.Minimum;
                    return;
                }

                if (!decimal.TryParse(t, out decimal v) || v < (decimal)nudQuantity.Minimum)
                {
                    nudQuantity.Value = nudQuantity.Minimum;
                }
            }
            catch
            {
                try { nudQuantity.Value = nudQuantity.Minimum; } catch { }
            }
        }

        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public int Quantity
        {
            get => (int)nudQuantity.Value;
            set => nudQuantity.Value = Math.Max(1, Math.Min(999, value));
        }

        public void SetData(int maMon, string ten, decimal price, string imagePath = null)
        {
            MaMon = maMon;
            TenMon = ten;
            Price = price;
            ImagePath = imagePath;
            lblTitle.Text = ten;
            lblPrice.Text = price.ToString("N0") + " VNĐ";

            // Load image using ImageHelper which supports relative paths under Images directory
            try
            {
                // Dispose previous image
                if (pbImage.Image != null)
                {
                    var old = pbImage.Image;
                    pbImage.Image = null;
                    old.Dispose();
                }

                Image img = null;
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    if (Path.IsPathRooted(imagePath))
                    {
                        if (File.Exists(imagePath)) img = Image.FromFile(imagePath);
                    }
                    else
                    {
                        img = ImageHelper.LoadMenuItemImage(imagePath);
                    }
                }

                if (img == null)
                {
                    // Try default image under Images/default.jpg
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    string defaultPath = Path.Combine(imagesDir, "default.jpg");
                    if (File.Exists(defaultPath)) img = Image.FromFile(defaultPath);
                }

                if (img != null)
                {
                    // copy image to avoid locking source file
                    pbImage.Image = new Bitmap(img);
                    img.Dispose();
                }
            }
            catch
            {
                // ignore image errors
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            OnAddClicked(new MenuItemEventArgs
            {
                MaMon = this.MaMon,
                TenMon = this.TenMon,
                Price = this.Price,
                Quantity = this.Quantity
            });
        }

        public event EventHandler<MenuItemEventArgs> AddClicked;

        protected virtual void OnAddClicked(MenuItemEventArgs e)
        {
            AddClicked?.Invoke(this, e);
        }
    }

    public class MenuItemEventArgs : EventArgs
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
