using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.Entities;
using PM_Ban_Do_An_Nhanh.UI;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmCart : Form
    {
        private readonly BindingList<ChiTietDonHang> _items;

        public IReadOnlyList<ChiTietDonHang> Items => _items.ToList();

        public frmCart(IEnumerable<ChiTietDonHang> items)
        {
            if (items == null) items = Enumerable.Empty<ChiTietDonHang>();

            _items = new BindingList<ChiTietDonHang>(
                items.Select(i => new ChiTietDonHang
                {
                    MaDH = i.MaDH,
                    MaMon = i.MaMon,
                    TenMon = i.TenMon,
                    SoLuong = i.SoLuong,
                    DonGia = i.DonGia
                }).ToList());

            InitializeComponent();

            dgvCart.CellContentClick += Dgv_CellContentClick;
            btnOk.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            BindData();
            UpdateTotal();
        }

        private void BindData()
        {
            dgvCart.Columns.Clear();
            dgvCart.AutoGenerateColumns = false;

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ChiTietDonHang.TenMon),
                HeaderText = "Món",
                FillWeight = 44
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ChiTietDonHang.SoLuong),
                HeaderText = "SL",
                FillWeight = 10
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ChiTietDonHang.DonGia),
                HeaderText = "Đơn giá",
                FillWeight = 16
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ChiTietDonHang.ThanhTien),
                HeaderText = "Thành tiền",
                FillWeight = 16
            });

            dgvCart.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colPlus",
                HeaderText = "",
                Text = "+",
                UseColumnTextForButtonValue = true,
                FillWeight = 7
            });

            dgvCart.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colMinus",
                HeaderText = "",
                Text = "-",
                UseColumnTextForButtonValue = true,
                FillWeight = 7
            });

            dgvCart.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colRemove",
                HeaderText = "",
                Text = "X",
                UseColumnTextForButtonValue = true,
                FillWeight = 7
            });

            dgvCart.DataSource = _items;
            _items.ListChanged += (s, e) => UpdateTotal();

            // VNĐ formatting
            TableStyleHelper.ApplyVndFormatting(dgvCart, nameof(ChiTietDonHang.DonGia), nameof(ChiTietDonHang.ThanhTien));
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = dgvCart.Columns[e.ColumnIndex].Name;
            if (colName != "colPlus" && colName != "colMinus" && colName != "colRemove") return;

            var item = _items[e.RowIndex];

            if (colName == "colPlus")
            {
                item.SoLuong += 1;
            }
            else if (colName == "colMinus")
            {
                if (item.SoLuong > 1) item.SoLuong -= 1;
            }
            else if (colName == "colRemove")
            {
                _items.RemoveAt(e.RowIndex);
            }

            dgvCart.Refresh();
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var total = _items.Sum(i => i.ThanhTien);
            lblTotal.Text = "Tổng tiền: " + TableStyleHelper.FormatVnd(total);
            btnOk.Enabled = _items.Count > 0;
        }
    }
}
