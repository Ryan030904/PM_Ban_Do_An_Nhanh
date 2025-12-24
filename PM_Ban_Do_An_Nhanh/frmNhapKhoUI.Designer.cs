namespace PM_Ban_Do_An_Nhanh
{
    partial class frmNhapKhoUI
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
            this.components = new System.ComponentModel.Container();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.grpCreate = new System.Windows.Forms.GroupBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblMon = new System.Windows.Forms.Label();
            this.cboMon = new System.Windows.Forms.ComboBox();
            this.lblSoLuong = new System.Windows.Forms.Label();
            this.txtSoLuong = new System.Windows.Forms.TextBox();
            this.lblDonGia = new System.Windows.Forms.Label();
            this.txtDonGia = new System.Windows.Forms.TextBox();
            this.btnThemDong = new System.Windows.Forms.Button();
            this.btnLuuPhieu = new System.Windows.Forms.Button();
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.txtGhiChu = new System.Windows.Forms.TextBox();
            this.dgvChiTietNhap = new System.Windows.Forms.DataGridView();
            this.grpList = new System.Windows.Forms.GroupBox();
            this.splitList = new System.Windows.Forms.SplitContainer();
            this.dgvDanhSachPhieu = new System.Windows.Forms.DataGridView();
            this.dgvChiTietPhieu = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.grpCreate.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietNhap)).BeginInit();
            this.grpList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitList)).BeginInit();
            this.splitList.Panel1.SuspendLayout();
            this.splitList.Panel2.SuspendLayout();
            this.splitList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDanhSachPhieu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietPhieu)).BeginInit();
            this.SuspendLayout();
            
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            
            this.splitMain.Panel1.Controls.Add(this.grpCreate);
            
            this.splitMain.Panel2.Controls.Add(this.grpList);
            this.splitMain.Size = new System.Drawing.Size(1100, 650);
            this.splitMain.SplitterDistance = 290;
            this.splitMain.TabIndex = 0;
            
            this.grpCreate.Controls.Add(this.dgvChiTietNhap);
            this.grpCreate.Controls.Add(this.pnlTop);
            this.grpCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCreate.Location = new System.Drawing.Point(0, 0);
            this.grpCreate.Name = "grpCreate";
            this.grpCreate.Padding = new System.Windows.Forms.Padding(8);
            this.grpCreate.Size = new System.Drawing.Size(1100, 290);
            this.grpCreate.TabIndex = 0;
            this.grpCreate.TabStop = false;
            this.grpCreate.Text = "Tạo phiếu nhập";
            
            this.pnlTop.Controls.Add(this.lblMon);
            this.pnlTop.Controls.Add(this.cboMon);
            this.pnlTop.Controls.Add(this.lblSoLuong);
            this.pnlTop.Controls.Add(this.txtSoLuong);
            this.pnlTop.Controls.Add(this.lblDonGia);
            this.pnlTop.Controls.Add(this.txtDonGia);
            this.pnlTop.Controls.Add(this.btnThemDong);
            this.pnlTop.Controls.Add(this.btnLuuPhieu);
            this.pnlTop.Controls.Add(this.lblGhiChu);
            this.pnlTop.Controls.Add(this.txtGhiChu);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(8, 27);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1084, 95);
            this.pnlTop.TabIndex = 0;
            
            this.lblMon.AutoSize = true;
            this.lblMon.Location = new System.Drawing.Point(12, 15);
            this.lblMon.Name = "lblMon";
            this.lblMon.Size = new System.Drawing.Size(40, 19);
            this.lblMon.TabIndex = 0;
            this.lblMon.Text = "Món:";
            
            this.cboMon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMon.FormattingEnabled = true;
            this.cboMon.Location = new System.Drawing.Point(90, 12);
            this.cboMon.Name = "cboMon";
            this.cboMon.Size = new System.Drawing.Size(320, 25);
            this.cboMon.TabIndex = 1;
            
            this.lblSoLuong.AutoSize = true;
            this.lblSoLuong.Location = new System.Drawing.Point(430, 15);
            this.lblSoLuong.Name = "lblSoLuong";
            this.lblSoLuong.Size = new System.Drawing.Size(65, 19);
            this.lblSoLuong.TabIndex = 2;
            this.lblSoLuong.Text = "Số lượng:";
            
            this.txtSoLuong.Location = new System.Drawing.Point(510, 12);
            this.txtSoLuong.Name = "txtSoLuong";
            this.txtSoLuong.Size = new System.Drawing.Size(90, 25);
            this.txtSoLuong.TabIndex = 3;
            
            this.lblDonGia.AutoSize = true;
            this.lblDonGia.Location = new System.Drawing.Point(620, 15);
            this.lblDonGia.Name = "lblDonGia";
            this.lblDonGia.Size = new System.Drawing.Size(57, 19);
            this.lblDonGia.TabIndex = 4;
            this.lblDonGia.Text = "Đơn giá:";
            
            this.txtDonGia.Location = new System.Drawing.Point(690, 12);
            this.txtDonGia.Name = "txtDonGia";
            this.txtDonGia.Size = new System.Drawing.Size(120, 25);
            this.txtDonGia.TabIndex = 5;
            
            this.btnThemDong.Location = new System.Drawing.Point(830, 10);
            this.btnThemDong.Name = "btnThemDong";
            this.btnThemDong.Size = new System.Drawing.Size(90, 29);
            this.btnThemDong.TabIndex = 6;
            this.btnThemDong.Text = "Thêm";
            this.btnThemDong.UseVisualStyleBackColor = true;
            
            this.btnLuuPhieu.Location = new System.Drawing.Point(930, 10);
            this.btnLuuPhieu.Name = "btnLuuPhieu";
            this.btnLuuPhieu.Size = new System.Drawing.Size(120, 29);
            this.btnLuuPhieu.TabIndex = 7;
            this.btnLuuPhieu.Text = "Lưu phiếu";
            this.btnLuuPhieu.UseVisualStyleBackColor = true;
            
            this.lblGhiChu.AutoSize = true;
            this.lblGhiChu.Location = new System.Drawing.Point(12, 58);
            this.lblGhiChu.Name = "lblGhiChu";
            this.lblGhiChu.Size = new System.Drawing.Size(55, 19);
            this.lblGhiChu.TabIndex = 8;
            this.lblGhiChu.Text = "Ghi chú:";
            
            this.txtGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhiChu.Location = new System.Drawing.Point(90, 55);
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.Size = new System.Drawing.Size(960, 25);
            this.txtGhiChu.TabIndex = 9;
            
            this.dgvChiTietNhap.AllowUserToAddRows = false;
            this.dgvChiTietNhap.AllowUserToDeleteRows = false;
            this.dgvChiTietNhap.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTietNhap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChiTietNhap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChiTietNhap.Location = new System.Drawing.Point(8, 122);
            this.dgvChiTietNhap.Name = "dgvChiTietNhap";
            this.dgvChiTietNhap.ReadOnly = true;
            this.dgvChiTietNhap.RowHeadersWidth = 51;
            this.dgvChiTietNhap.Size = new System.Drawing.Size(1084, 160);
            this.dgvChiTietNhap.TabIndex = 1;
            
            this.grpList.Controls.Add(this.splitList);
            this.grpList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpList.Location = new System.Drawing.Point(0, 0);
            this.grpList.Name = "grpList";
            this.grpList.Padding = new System.Windows.Forms.Padding(8);
            this.grpList.Size = new System.Drawing.Size(1100, 356);
            this.grpList.TabIndex = 0;
            this.grpList.TabStop = false;
            this.grpList.Text = "Danh sách phiếu nhập";
            
            this.splitList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitList.Location = new System.Drawing.Point(8, 27);
            this.splitList.Name = "splitList";
            
            this.splitList.Panel1.Controls.Add(this.dgvDanhSachPhieu);
            
            this.splitList.Panel2.Controls.Add(this.dgvChiTietPhieu);
            this.splitList.Size = new System.Drawing.Size(1084, 321);
            this.splitList.SplitterDistance = 520;
            this.splitList.TabIndex = 0;
            
            this.dgvDanhSachPhieu.AllowUserToAddRows = false;
            this.dgvDanhSachPhieu.AllowUserToDeleteRows = false;
            this.dgvDanhSachPhieu.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDanhSachPhieu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDanhSachPhieu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDanhSachPhieu.Location = new System.Drawing.Point(0, 0);
            this.dgvDanhSachPhieu.Name = "dgvDanhSachPhieu";
            this.dgvDanhSachPhieu.ReadOnly = true;
            this.dgvDanhSachPhieu.RowHeadersWidth = 51;
            this.dgvDanhSachPhieu.Size = new System.Drawing.Size(520, 321);
            this.dgvDanhSachPhieu.TabIndex = 0;
            
            this.dgvChiTietPhieu.AllowUserToAddRows = false;
            this.dgvChiTietPhieu.AllowUserToDeleteRows = false;
            this.dgvChiTietPhieu.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTietPhieu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChiTietPhieu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChiTietPhieu.Location = new System.Drawing.Point(0, 0);
            this.dgvChiTietPhieu.Name = "dgvChiTietPhieu";
            this.dgvChiTietPhieu.ReadOnly = true;
            this.dgvChiTietPhieu.RowHeadersWidth = 51;
            this.dgvChiTietPhieu.Size = new System.Drawing.Size(560, 321);
            this.dgvChiTietPhieu.TabIndex = 0;
            
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.splitMain);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(980, 560);
            this.Name = "frmNhapKhoUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nhập kho";
            
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.grpCreate.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietNhap)).EndInit();
            this.grpList.ResumeLayout(false);
            this.splitList.Panel1.ResumeLayout(false);
            this.splitList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitList)).EndInit();
            this.splitList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDanhSachPhieu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietPhieu)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox grpCreate;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblMon;
        private System.Windows.Forms.ComboBox cboMon;
        private System.Windows.Forms.Label lblSoLuong;
        private System.Windows.Forms.TextBox txtSoLuong;
        private System.Windows.Forms.Label lblDonGia;
        private System.Windows.Forms.TextBox txtDonGia;
        private System.Windows.Forms.Button btnThemDong;
        private System.Windows.Forms.Button btnLuuPhieu;
        private System.Windows.Forms.Label lblGhiChu;
        private System.Windows.Forms.TextBox txtGhiChu;
        private System.Windows.Forms.DataGridView dgvChiTietNhap;
        private System.Windows.Forms.GroupBox grpList;
        private System.Windows.Forms.SplitContainer splitList;
        private System.Windows.Forms.DataGridView dgvDanhSachPhieu;
        private System.Windows.Forms.DataGridView dgvChiTietPhieu;
    }
}