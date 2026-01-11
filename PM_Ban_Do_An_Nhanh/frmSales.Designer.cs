using System;

namespace PM_Ban_Do_An_Nhanh
{
    partial class frmSales
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage tabOrder;
        private System.Windows.Forms.TabPage tabCustomer;
        private System.Windows.Forms.TabPage tabHistory;
        private System.Windows.Forms.FlowLayoutPanel pnlMonAn;
        private System.Windows.Forms.DataGridView dgvOrderList;
        private System.Windows.Forms.DataGridView dgvKhachHang;
        private System.Windows.Forms.DataGridView dgvHoaDon;
        private System.Windows.Forms.Label lblTongTien;
        private System.Windows.Forms.Label lblTenKhachHang;
        private System.Windows.Forms.TextBox txtSDTKhachHang;
        private System.Windows.Forms.Button btnTimKH;
        private System.Windows.Forms.Button btnXoaKH;
        private System.Windows.Forms.Button btnPlusItem;
        private System.Windows.Forms.Button btnMinusItem;
        private System.Windows.Forms.Button btnRemoveItem;
        private System.Windows.Forms.Button btnThanhToan;
        private System.Windows.Forms.Button btnHuyDon;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnExportPdf;
        private System.Windows.Forms.TextBox txtSearchMenu;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.tabOrder = new System.Windows.Forms.TabPage();
            this.tabCustomer = new System.Windows.Forms.TabPage();
            this.tabHistory = new System.Windows.Forms.TabPage();
            this.pnlMonAn = new System.Windows.Forms.FlowLayoutPanel();
            this.dgvOrderList = new System.Windows.Forms.DataGridView();
            this.dgvKhachHang = new System.Windows.Forms.DataGridView();
            this.dgvHoaDon = new System.Windows.Forms.DataGridView();
            this.lblTongTien = new System.Windows.Forms.Label();
            this.lblTenKhachHang = new System.Windows.Forms.Label();
            this.txtSDTKhachHang = new System.Windows.Forms.TextBox();
            this.btnTimKH = new System.Windows.Forms.Button();
            this.btnXoaKH = new System.Windows.Forms.Button();
            this.btnPlusItem = new System.Windows.Forms.Button();
            this.btnMinusItem = new System.Windows.Forms.Button();
            this.btnRemoveItem = new System.Windows.Forms.Button();
            this.btnThanhToan = new System.Windows.Forms.Button();
            this.btnHuyDon = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.txtSearchMenu = new System.Windows.Forms.TextBox();

            this.mainTabControl.SuspendLayout();
            this.tabOrder.SuspendLayout();
            this.tabCustomer.SuspendLayout();
            this.tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrderList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHoaDon)).BeginInit();
            this.SuspendLayout();

            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.tabOrder);
            this.mainTabControl.Controls.Add(this.tabCustomer);
            this.mainTabControl.Controls.Add(this.tabHistory);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.mainTabControl.ItemSize = new System.Drawing.Size(120, 40);
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(1400, 800);
            this.mainTabControl.TabIndex = 0;

            // 
            // tabOrder
            // 
            this.tabOrder.BackColor = System.Drawing.SystemColors.Control;
            this.tabOrder.Location = new System.Drawing.Point(4, 44);
            this.tabOrder.Name = "tabOrder";
            this.tabOrder.Padding = new System.Windows.Forms.Padding(16);
            this.tabOrder.Size = new System.Drawing.Size(1392, 752);
            this.tabOrder.TabIndex = 0;
            this.tabOrder.Text = "Đặt hàng";

            // Ensure pnlMonAn is added to tabOrder and fills the area so menu items are visible
            this.pnlMonAn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOrder.Controls.Add(this.pnlMonAn);

            // 
            // tabCustomer
            // 
            this.tabCustomer.Controls.Add(this.dgvKhachHang);
            this.tabCustomer.BackColor = System.Drawing.SystemColors.Control;
            this.tabCustomer.Location = new System.Drawing.Point(4, 44);
            this.tabCustomer.Name = "tabCustomer";
            this.tabCustomer.Padding = new System.Windows.Forms.Padding(16);
            this.tabCustomer.Size = new System.Drawing.Size(1392, 752);
            this.tabCustomer.TabIndex = 1;
            this.tabCustomer.Text = "Khách hàng";

            // 
            // tabHistory
            // 
            this.tabHistory.Controls.Add(this.dgvHoaDon);
            this.tabHistory.BackColor = System.Drawing.SystemColors.Control;
            this.tabHistory.Location = new System.Drawing.Point(4, 44);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Padding = new System.Windows.Forms.Padding(16);
            this.tabHistory.Size = new System.Drawing.Size(1392, 752);
            this.tabHistory.TabIndex = 2;
            this.tabHistory.Text = "Lịch sử";

            // 
            // pnlMonAn
            // 
            this.pnlMonAn.AutoScroll = true;
            this.pnlMonAn.BackColor = System.Drawing.SystemColors.Control;
            this.pnlMonAn.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlMonAn.WrapContents = true;
            this.pnlMonAn.Name = "pnlMonAn";
            this.pnlMonAn.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMonAn.TabIndex = 0;

            // 
            // dgvOrderList
            // 
            this.dgvOrderList.AllowUserToAddRows = false;
            this.dgvOrderList.AllowUserToDeleteRows = false;
            this.dgvOrderList.BackgroundColor = System.Drawing.Color.White;
            this.dgvOrderList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvOrderList.ColumnHeadersHeight = 45;
            this.dgvOrderList.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dgvOrderList.MultiSelect = false;
            this.dgvOrderList.Name = "dgvOrderList";
            this.dgvOrderList.ReadOnly = true;
            this.dgvOrderList.RowHeadersVisible = false;
            this.dgvOrderList.RowTemplate.Height = 35;
            this.dgvOrderList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOrderList.TabIndex = 1;

            // 
            // dgvKhachHang
            // 
            this.dgvKhachHang.AllowUserToAddRows = false;
            this.dgvKhachHang.AllowUserToDeleteRows = false;
            this.dgvKhachHang.BackgroundColor = System.Drawing.Color.White;
            this.dgvKhachHang.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvKhachHang.ColumnHeadersHeight = 45;
            this.dgvKhachHang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKhachHang.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dgvKhachHang.MultiSelect = false;
            this.dgvKhachHang.Name = "dgvKhachHang";
            this.dgvKhachHang.ReadOnly = true;
            this.dgvKhachHang.RowHeadersVisible = false;
            this.dgvKhachHang.RowTemplate.Height = 35;
            this.dgvKhachHang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKhachHang.TabIndex = 2;
            this.dgvKhachHang.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvKhachHang_CellClick);

            // 
            // dgvHoaDon
            // 
            this.dgvHoaDon.AllowUserToAddRows = false;
            this.dgvHoaDon.AllowUserToDeleteRows = false;
            this.dgvHoaDon.BackgroundColor = System.Drawing.Color.White;
            this.dgvHoaDon.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvHoaDon.ColumnHeadersHeight = 45;
            this.dgvHoaDon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHoaDon.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.dgvHoaDon.MultiSelect = false;
            this.dgvHoaDon.Name = "dgvHoaDon";
            this.dgvHoaDon.ReadOnly = true;
            this.dgvHoaDon.RowHeadersVisible = false;
            this.dgvHoaDon.RowTemplate.Height = 35;
            this.dgvHoaDon.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHoaDon.TabIndex = 3;
            this.dgvHoaDon.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHoaDon_CellClick);

             this.lblTongTien.Text = "0 VNĐ";
             this.lblTongTien.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
             this.lblTongTien.ForeColor = System.Drawing.Color.Black;

             this.lblTenKhachHang.Text = "Khách lẻ";
             this.lblTenKhachHang.Font = new System.Drawing.Font("Arial", 10F);

             this.txtSDTKhachHang.Font = new System.Drawing.Font("Arial", 10F);

             this.txtSearchMenu.Font = new System.Drawing.Font("Arial", 10F);

             // Buttons basic setup
             this.btnTimKH.Text = "Tìm kiếm";
             this.btnTimKH.Click += new System.EventHandler(this.btnTimKH_Click);

             this.btnXoaKH.Text = "Xóa chọn";
             this.btnXoaKH.Click += new System.EventHandler(this.btnXoaKH_Click);

             this.btnPlusItem.Text = "+";
             this.btnPlusItem.Click += new System.EventHandler(this.btnPlusItem_Click);

             this.btnMinusItem.Text = "-";
             this.btnMinusItem.Click += new System.EventHandler(this.btnMinusItem_Click);

             this.btnRemoveItem.Text = "Xóa";
             this.btnRemoveItem.Click += new System.EventHandler(this.btnRemoveItem_Click);

             this.btnThanhToan.Text = "THANH TOÁN";
             this.btnThanhToan.Click += new System.EventHandler(this.btnThanhToan_Click);

             this.btnHuyDon.Text = "HỦY ĐƠN";
             this.btnHuyDon.Click += new System.EventHandler(this.btnHuyDon_Click);

             this.btnPrint.Text = "In hóa đơn";
             this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);

             this.btnExportPdf.Text = "Xuất PDF";
             this.btnExportPdf.Click += new System.EventHandler(this.btnExportPdf_Click);

            // Form properties
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.mainTabControl);
            this.Font = new System.Drawing.Font("Arial", 10F);
            this.MinimumSize = new System.Drawing.Size(1200, 600);
            this.Name = "frmSales";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bán hàng";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.mainTabControl.ResumeLayout(false);
            this.tabOrder.ResumeLayout(false);
            this.tabCustomer.ResumeLayout(false);
            this.tabHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOrderList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHoaDon)).EndInit();
            this.ResumeLayout(false);
        }
    







        private System.Windows.Forms.Button CreateActionButton(string text, System.Drawing.Color backgroundColor, string tooltip = "")
        {
            System.Windows.Forms.Button btn = new System.Windows.Forms.Button();
            btn.Text = text;
            btn.Size = new System.Drawing.Size(50, 35);
            btn.BackColor = backgroundColor;
            btn.ForeColor = System.Drawing.Color.White;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new System.Drawing.Font("Segoe UI", 12F);
            btn.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btn.Cursor = System.Windows.Forms.Cursors.Hand;

            // Add tooltip if provided
            if (!string.IsNullOrEmpty(tooltip))
            {
                System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
                toolTip.SetToolTip(btn, tooltip);
            }

            // Add hover effects
            btn.MouseEnter += (s, e) => {
                btn.BackColor = System.Drawing.Color.FromArgb(
                    Math.Min(255, backgroundColor.R + 20),
                    Math.Min(255, backgroundColor.G + 20),
                    Math.Min(255, backgroundColor.B + 20)
                );
            };
            btn.MouseLeave += (s, e) => btn.BackColor = backgroundColor;

            if (text == "➕") btn.Click += new System.EventHandler(this.btnPlusItem_Click);
            if (text == "➖") btn.Click += new System.EventHandler(this.btnMinusItem_Click);
            if (text == "🗑️") btn.Click += new System.EventHandler(this.btnRemoveItem_Click);

            return btn;
        }





        private void ApplyModernGridStyle(System.Windows.Forms.DataGridView grid, string theme)
        {
            System.Drawing.Color headerColor, accentColor;

            switch (theme)
            {
                case "success":
                    headerColor = System.Drawing.Color.FromArgb(46, 204, 113);
                    accentColor = System.Drawing.Color.FromArgb(26, 188, 156);
                    break;
                case "warning":
                    headerColor = System.Drawing.Color.FromArgb(230, 126, 34);
                    accentColor = System.Drawing.Color.FromArgb(243, 156, 18);
                    break;
                case "danger":
                    headerColor = System.Drawing.Color.FromArgb(231, 76, 60);
                    accentColor = System.Drawing.Color.FromArgb(192, 57, 43);
                    break;
                default:
                    headerColor = System.Drawing.Color.FromArgb(52, 152, 219);
                    accentColor = System.Drawing.Color.FromArgb(41, 128, 185);
                    break;
            }

            // Header style
            grid.ColumnHeadersDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = headerColor,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = headerColor,
                Padding = new System.Windows.Forms.Padding(8, 8, 8, 8)
            };

            // Cell style
            grid.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(44, 62, 80),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                SelectionBackColor = System.Drawing.Color.FromArgb(174, 214, 241),
                SelectionForeColor = System.Drawing.Color.FromArgb(44, 62, 80),
                Padding = new System.Windows.Forms.Padding(8, 4, 8, 4)
            };

            // Alternating row style
            grid.AlternatingRowsDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(248, 249, 250),
                SelectionBackColor = System.Drawing.Color.FromArgb(174, 214, 241)
            };
        }
    }
}
