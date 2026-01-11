namespace PM_Ban_Do_An_Nhanh
{
    partial class frmReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnInBaoCao = new System.Windows.Forms.Button();
            this.lblTotalRevenue = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnXemBaoCao = new System.Windows.Forms.Button();
            this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
            this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControlReports = new System.Windows.Forms.TabControl();
            this.tabPageDoanhThu = new System.Windows.Forms.TabPage();
            this.dgvDoanhThu = new System.Windows.Forms.DataGridView();
            this.Ngay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DoanhThuNgay = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageMonBanChay = new System.Windows.Forms.TabPage();
            this.dgvMonBanChay = new System.Windows.Forms.DataGridView();
            this.TenMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TongSoLuongBan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TongDoanhThuMon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrintReportPage = new System.Drawing.Printing.PrintDocument();
            this.panel1.SuspendLayout();
            this.tabControlReports.SuspendLayout();
            this.tabPageDoanhThu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDoanhThu)).BeginInit();
            this.tabPageMonBanChay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonBanChay)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnInBaoCao);
            this.panel1.Controls.Add(this.lblTotalRevenue);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnXemBaoCao);
            this.panel1.Controls.Add(this.dtpDenNgay);
            this.panel1.Controls.Add(this.dtpTuNgay);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(170, 366);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // btnInBaoCao
            // 
            this.btnInBaoCao.Location = new System.Drawing.Point(11, 325);
            this.btnInBaoCao.Name = "btnInBaoCao";
            this.btnInBaoCao.Size = new System.Drawing.Size(150, 35);
            this.btnInBaoCao.TabIndex = 7;
            this.btnInBaoCao.Text = "In báo cáo";
            this.btnInBaoCao.UseVisualStyleBackColor = true;
            this.btnInBaoCao.Click += new System.EventHandler(this.btnInBaoCao_Click);
            // 
            // lblTotalRevenue
            // 
            this.lblTotalRevenue.Location = new System.Drawing.Point(29, 240);
            this.lblTotalRevenue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTotalRevenue.Name = "lblTotalRevenue";
            this.lblTotalRevenue.Size = new System.Drawing.Size(130, 35);
            this.lblTotalRevenue.TabIndex = 6;
            this.lblTotalRevenue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTotalRevenue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTotalRevenue.BackColor = System.Drawing.Color.FromArgb(248, 255, 248);
            this.lblTotalRevenue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalRevenue.ForeColor = System.Drawing.Color.Green;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 210);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Tổng doanh thu:";
            // 
            // btnXemBaoCao
            // 
            this.btnXemBaoCao.AutoSize = false;
            this.btnXemBaoCao.Location = new System.Drawing.Point(11, 285);
            this.btnXemBaoCao.Margin = new System.Windows.Forms.Padding(2);
            this.btnXemBaoCao.Name = "btnXemBaoCao";
            this.btnXemBaoCao.Size = new System.Drawing.Size(150, 35);
            this.btnXemBaoCao.TabIndex = 4;
            this.btnXemBaoCao.Text = "Xem báo cáo";
            this.btnXemBaoCao.UseVisualStyleBackColor = true;
            this.btnXemBaoCao.Click += new System.EventHandler(this.btnXemBaoCao_Click);
            // 
            // dtpDenNgay
            // 
            this.dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDenNgay.Location = new System.Drawing.Point(26, 155);
            this.dtpDenNgay.Margin = new System.Windows.Forms.Padding(2);
            this.dtpDenNgay.Name = "dtpDenNgay";
            this.dtpDenNgay.ShowCheckBox = true;
            this.dtpDenNgay.Size = new System.Drawing.Size(135, 23);
            this.dtpDenNgay.TabIndex = 3;
            // 
            // dtpTuNgay
            // 
            this.dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTuNgay.Location = new System.Drawing.Point(26, 72);
            this.dtpTuNgay.Margin = new System.Windows.Forms.Padding(2);
            this.dtpTuNgay.Name = "dtpTuNgay";
            this.dtpTuNgay.ShowCheckBox = true;
            this.dtpTuNgay.Size = new System.Drawing.Size(135, 23);
            this.dtpTuNgay.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 128);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Đến ngày";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Từ ngày";
            // 
            // tabControlReports
            // 
            this.tabControlReports.Controls.Add(this.tabPageDoanhThu);
            this.tabControlReports.Controls.Add(this.tabPageMonBanChay);
            this.tabControlReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlReports.Location = new System.Drawing.Point(152, 0);
            this.tabControlReports.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlReports.Name = "tabControlReports";
            this.tabControlReports.SelectedIndex = 0;
            this.tabControlReports.Size = new System.Drawing.Size(419, 366);
            this.tabControlReports.TabIndex = 1;
            // 
            // tabPageDoanhThu
            // 
            this.tabPageDoanhThu.Controls.Add(this.dgvDoanhThu);
            this.tabPageDoanhThu.Location = new System.Drawing.Point(4, 22);
            this.tabPageDoanhThu.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageDoanhThu.Name = "tabPageDoanhThu";
            this.tabPageDoanhThu.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageDoanhThu.Size = new System.Drawing.Size(411, 340);
            this.tabPageDoanhThu.TabIndex = 0;
            this.tabPageDoanhThu.Text = "Doanh thu";
            this.tabPageDoanhThu.UseVisualStyleBackColor = true;
            // 
            // dgvDoanhThu
            // 
            this.dgvDoanhThu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDoanhThu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Ngay,
            this.DoanhThuNgay});
            this.dgvDoanhThu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDoanhThu.Location = new System.Drawing.Point(2, 2);
            this.dgvDoanhThu.Margin = new System.Windows.Forms.Padding(2);
            this.dgvDoanhThu.Name = "dgvDoanhThu";
            this.dgvDoanhThu.RowHeadersWidth = 51;
            this.dgvDoanhThu.RowTemplate.Height = 24;
            this.dgvDoanhThu.Size = new System.Drawing.Size(407, 336);
            this.dgvDoanhThu.TabIndex = 0;
            // 
            // Ngay
            // 
            this.Ngay.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Ngay.DataPropertyName = "Ngay";
            this.Ngay.HeaderText = "Ngày";
            this.Ngay.MinimumWidth = 6;
            this.Ngay.Name = "Ngay";
            this.Ngay.ReadOnly = true;
            // 
            // DoanhThuNgay
            // 
            this.DoanhThuNgay.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DoanhThuNgay.DataPropertyName = "DoanhThuNgay";
            this.DoanhThuNgay.HeaderText = "DoanhThu";
            this.DoanhThuNgay.MinimumWidth = 6;
            this.DoanhThuNgay.Name = "DoanhThuNgay";
            this.DoanhThuNgay.ReadOnly = true;
            // 
            // tabPageMonBanChay
            // 
            this.tabPageMonBanChay.Controls.Add(this.dgvMonBanChay);
            this.tabPageMonBanChay.Location = new System.Drawing.Point(4, 22);
            this.tabPageMonBanChay.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageMonBanChay.Name = "tabPageMonBanChay";
            this.tabPageMonBanChay.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageMonBanChay.Size = new System.Drawing.Size(411, 340);
            this.tabPageMonBanChay.TabIndex = 1;
            this.tabPageMonBanChay.Text = "Món bán chạy";
            this.tabPageMonBanChay.UseVisualStyleBackColor = true;
            // 
            // dgvMonBanChay
            // 
            this.dgvMonBanChay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonBanChay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TenMon,
            this.TongSoLuongBan,
            this.TongDoanhThuMon});
            this.dgvMonBanChay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMonBanChay.Location = new System.Drawing.Point(2, 2);
            this.dgvMonBanChay.Margin = new System.Windows.Forms.Padding(2);
            this.dgvMonBanChay.Name = "dgvMonBanChay";
            this.dgvMonBanChay.RowHeadersWidth = 51;
            this.dgvMonBanChay.RowTemplate.Height = 24;
            this.dgvMonBanChay.Size = new System.Drawing.Size(407, 336);
            this.dgvMonBanChay.TabIndex = 0;
            // 
            // TenMon
            // 
            this.TenMon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TenMon.DataPropertyName = "TenMon";
            this.TenMon.HeaderText = "Tên món";
            this.TenMon.MinimumWidth = 6;
            this.TenMon.Name = "TenMon";
            this.TenMon.ReadOnly = true;
            // 
            // TongSoLuongBan
            // 
            this.TongSoLuongBan.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TongSoLuongBan.DataPropertyName = "TongSoLuongBan";
            this.TongSoLuongBan.HeaderText = "Tổng SL Bán";
            this.TongSoLuongBan.MinimumWidth = 6;
            this.TongSoLuongBan.Name = "TongSoLuongBan";
            this.TongSoLuongBan.ReadOnly = true;
            // 
            // TongDoanhThuMon
            // 
            this.TongDoanhThuMon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TongDoanhThuMon.DataPropertyName = "TongDoanhThuMon";
            this.TongDoanhThuMon.HeaderText = "Tổng doanh thu";
            this.TongDoanhThuMon.MinimumWidth = 6;
            this.TongDoanhThuMon.Name = "TongDoanhThuMon";
            this.TongDoanhThuMon.ReadOnly = true;
            // 
            // PrintReportPage

            // 
            this.PrintReportPage.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintReportPage_PrintPage);
            // 
            // frmReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 366);
            this.Controls.Add(this.tabControlReports);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmReport";
            this.Text = "frmReport";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControlReports.ResumeLayout(false);
            this.tabPageDoanhThu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDoanhThu)).EndInit();
            this.tabPageMonBanChay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonBanChay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DateTimePicker dtpDenNgay;
        private System.Windows.Forms.DateTimePicker dtpTuNgay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnXemBaoCao;
        private System.Windows.Forms.TabControl tabControlReports;
        private System.Windows.Forms.TabPage tabPageDoanhThu;
        private System.Windows.Forms.DataGridView dgvDoanhThu;
        private System.Windows.Forms.TabPage tabPageMonBanChay;
        private System.Windows.Forms.DataGridView dgvMonBanChay;
        private System.Windows.Forms.DataGridViewTextBoxColumn TenMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn TongSoLuongBan;
        private System.Windows.Forms.DataGridViewTextBoxColumn TongDoanhThuMon;
        private System.Windows.Forms.DataGridViewTextBoxColumn Ngay;
        private System.Windows.Forms.DataGridViewTextBoxColumn DoanhThuNgay;
        private System.Windows.Forms.Label lblTotalRevenue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnInBaoCao;
        private System.Drawing.Printing.PrintDocument PrintReportPage;
    }
}