using PM_Ban_Do_An_Nhanh.BLL;
using PM_Ban_Do_An_Nhanh.Entities;
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

        private List<ChiTietDonHang> currentOrderItems = new List<ChiTietDonHang>();
        private KhachHang selectedCustomer = null;
        private int? selectedMaDH = null;

        private bool isProcessingPayment = false;
        private DateTime lastPaymentTime = DateTime.MinValue;

        public frmSales()
        {
            InitializeComponent();
            Text = "🍔 Hệ thống bán hàng - FastFood Manager";

            Load += frmSales_Load;
            btnThanhToan.Click += btnThanhToan_Click;
            btnHuyDon.Click += btnHuyDon_Click;
            btnPlusItem.Click += btnPlusItem_Click;
            btnMinusItem.Click += btnMinusItem_Click;
            btnRemoveItem.Click += btnRemoveItem_Click;
            btnTimKH.Click += btnTimKH_Click;
            btnXoaKH.Click += btnXoaKH_Click;
            btnPrint.Click += btnPrint_Click;
            btnExportPdf.Click += btnExportPdf_Click;

            dgvKhachHang.CellClick += dgvKhachHang_CellClick;
            dgvHoaDon.CellClick += dgvHoaDon_CellClick;
        }

        // ================= LOAD =================
        private async void frmSales_Load(object sender, EventArgs e)
        {
            SetupOrderGrid();
            ClearOrder();

            // Load data asynchronously to avoid UI freeze
            await LoadMonAnAsync();
            LoadKhachHang();
            LoadHoaDon();
        }

        // ================= MÓN ĂN =================
        private async Task LoadMonAnAsync()
        {
            pnlMonAn.Controls.Clear();

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
                        MenuItemCard card = new MenuItemCard();
                        card.SetData(
                            Convert.ToInt32(r["MaMon"]),
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
        }

        // ================= ORDER =================
        private void SetupOrderGrid()
        {
            dgvOrderList.Columns.Clear();
            dgvOrderList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrderList.ReadOnly = true;
            dgvOrderList.AllowUserToAddRows = false;

            dgvOrderList.Columns.Add("MaMon", "Mã");
            dgvOrderList.Columns["MaMon"].Visible = false;
            dgvOrderList.Columns.Add("TenMon", "Tên món");
            dgvOrderList.Columns.Add("SoLuong", "SL");
            dgvOrderList.Columns.Add("DonGia", "Đơn giá");
            dgvOrderList.Columns.Add("ThanhTien", "Thành tiền");
        }

        private void AddItemToOrder(int maMon, string tenMon, decimal gia, int soLuong)
        {
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
                dgvOrderList.Rows.Add(i.MaMon, i.TenMon, i.SoLuong, i.DonGia, tt);
                total += tt;
            }

            lblTongTien.Text = total.ToString("N0") + " VNĐ";
        }

        private void ClearOrder()
        {
            currentOrderItems.Clear();
            RefreshOrderGrid();
            selectedCustomer = null;
            lblTenKhachHang.Text = "Khách lẻ";
        }

        // ================= BUTTON ORDER =================
        private void btnPlusItem_Click(object sender, EventArgs e)
        {
            if (dgvOrderList.CurrentRow == null) return;
            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            currentOrderItems.First(x => x.MaMon == maMon).SoLuong++;
            RefreshOrderGrid();
        }

        private void btnMinusItem_Click(object sender, EventArgs e)
        {
            if (dgvOrderList.CurrentRow == null) return;
            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            ChiTietDonHang item = currentOrderItems.First(x => x.MaMon == maMon);
            if (item.SoLuong > 1) item.SoLuong--;
            RefreshOrderGrid();
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvOrderList.CurrentRow == null) return;
            int maMon = Convert.ToInt32(dgvOrderList.CurrentRow.Cells["MaMon"].Value);
            currentOrderItems.RemoveAll(x => x.MaMon == maMon);
            RefreshOrderGrid();
        }

        private void btnHuyDon_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Hủy đơn hàng?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                ClearOrder();
        }

        // ================= THANH TOÁN =================
        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (isProcessingPayment) return;
            isProcessingPayment = true;

            try
            {
                if (currentOrderItems.Count == 0)
                {
                    MessageBox.Show("Chưa có món!");
                    return;
                }

                decimal tongTien = currentOrderItems.Sum(x => x.SoLuong * x.DonGia);

                DonHang dh = new DonHang
                {
                    NgayLap = DateTime.Now,
                    TongTien = tongTien,
                    TrangThaiThanhToan = "Đã thanh toán",
                    MaKH = selectedCustomer != null ? (int?)selectedCustomer.MaKH : null
                };

                int maDH = donHangBLL.ThemDonHang(dh, currentOrderItems);
                MessageBox.Show("Thanh toán thành công!");
                InHoaDon(maDH);
                ClearOrder();
                LoadHoaDon();
            }
            finally
            {
                isProcessingPayment = false;
            }
        }

        // ================= KHÁCH HÀNG =================
        private void LoadKhachHang()
        {
            dgvKhachHang.DataSource = khachHangBLL.HienThiDanhSachKhachHang();
        }

        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgvKhachHang.Rows[e.RowIndex];
            selectedCustomer = new KhachHang
            {
                MaKH = Convert.ToInt32(r.Cells["MaKH"].Value),
                TenKH = r.Cells["TenKH"].Value.ToString(),
                SDT = r.Cells["SDT"].Value.ToString()
            };
            lblTenKhachHang.Text = selectedCustomer.TenKH;
            mainTabControl.SelectedTab = tabOrder;
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
                DataTable dt = khachHangBLL.HienThiDanhSachKhachHang();

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
            lblTenKhachHang.Text = "Khách lẻ";
        }

        // ================= HÓA ĐƠN =================
        private void LoadHoaDon()
        {
            dgvHoaDon.DataSource = donHangBLL.LayDanhSachDonHang();
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
