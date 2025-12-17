using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class MenuItemCard : UserControl
    {
        public MenuItemCard()
        {
            InitializeComponent();
            this.btnAdd.Click += BtnAdd_Click;
            this.DoubleBuffered = true;
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

            try
            {
                string finalPath = null;
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    finalPath = imagePath;
                }
                else
                {
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    string defaultPath = Path.Combine(imagesDir, "default.jpg");
                    if (File.Exists(defaultPath)) finalPath = defaultPath;
                }

                if (!string.IsNullOrEmpty(finalPath) && File.Exists(finalPath))
                {
                    using (var img = Image.FromFile(finalPath))
                    {
                        // copy image to avoid locking the file
                        pbImage.Image = new Bitmap(img);
                    }
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
