using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmMain : Form
    {
        private TabPage tabPageDanhMuc;
        private TabPage tabPageNhapKho;
        private TabPage tabPageXuatKho;

        private Button btnNhapKho;
        private Button btnXuatKho;

        public frmMain()
        {
            InitializeComponent();
            this.Text = "Hệ thống quản lý bán thức ăn nhanh";

            tabPageDanhMuc = new TabPage("Danh Mục");
            tabControlMain.TabPages.Add(tabPageDanhMuc);

            tabPageNhapKho = new TabPage("Nhập kho");
            tabControlMain.TabPages.Add(tabPageNhapKho);

            tabPageXuatKho = new TabPage("Xuất kho");
            tabControlMain.TabPages.Add(tabPageXuatKho);

            TaoNutKho();

            HienThiThongTinNguoiDung();
        }

        private void TaoNutKho()
        {
            btnNhapKho = new Button();
            btnNhapKho.Dock = DockStyle.Top;
            btnNhapKho.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(163)));
            btnNhapKho.Height = 46;
            btnNhapKho.Text = "Nhập kho";
            btnNhapKho.Enabled = false;
            btnNhapKho.Click += btnNhapKho_Click;

            btnXuatKho = new Button();
            btnXuatKho.Dock = DockStyle.Top;
            btnXuatKho.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(163)));
            btnXuatKho.Height = 46;
            btnXuatKho.Text = "Xuất kho";
            btnXuatKho.Enabled = false;
            btnXuatKho.Click += btnXuatKho_Click;

            panel1.Controls.Add(btnXuatKho);
            panel1.Controls.Add(btnNhapKho);
        }

        private void HienThiThongTinNguoiDung()
        {
            if (GlobalVariables.LoggedInUser != null)
            {
                lblUserInfo.Text = $"Xin chào, {GlobalVariables.LoggedInUser.TenTK}";
                btnSales.Enabled = true;
                btnMenuManagement.Enabled = true;
                btnReport.Enabled = true;

                if (btnNhapKho != null) btnNhapKho.Enabled = true;
                if (btnXuatKho != null) btnXuatKho.Enabled = true;
            }
            else
            {
                MessageBox.Show("Phiên làm việc hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
            }
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            frmSales salesForm = new frmSales();
            LoadFormInTabPage(salesForm, tabPageSales);
            tabControlMain.SelectedTab = tabPageSales;
        }

        private void btnKhachHang_Click(object sender, EventArgs e)
        {
            frmAddCustomer khachHangForm = new frmAddCustomer();
            LoadFormInTabPage(khachHangForm, tabPageCustomer);
            tabControlMain.SelectedTab = tabPageCustomer;
        }

        private void btnMenuManagement_Click(object sender, EventArgs e)
        {
            frmMenuManagement menuForm = new frmMenuManagement();
            LoadFormInTabPage(menuForm, tabPageMenu);
            tabControlMain.SelectedTab = tabPageMenu;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            frmReport reportForm = new frmReport();
            LoadFormInTabPage(reportForm, tabPageReport);
            tabControlMain.SelectedTab = tabPageReport;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống không?", "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                GlobalVariables.LoggedInUser = null;
                this.Hide();
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void LoadFormInTabPage(Form form, TabPage tabPage)
        {
            tabPage.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            tabPage.Controls.Add(form);
            form.Show();
        }

        private void btnDanhMuc_Click(object sender, EventArgs e)
        {
            frmDanhMuc danhmucForm = new frmDanhMuc();
            LoadFormInTabPage(danhmucForm, tabPageDanhMuc);
            tabControlMain.SelectedTab = tabPageDanhMuc;
        }

        private void btnNhapKho_Click(object sender, EventArgs e)
        {
            frmNhapKhoUI form = new frmNhapKhoUI();
            LoadFormInTabPage(form, tabPageNhapKho);
            tabControlMain.SelectedTab = tabPageNhapKho;
        }

        private void btnXuatKho_Click(object sender, EventArgs e)
        {
            frmXuatKhoUI form = new frmXuatKhoUI();
            LoadFormInTabPage(form, tabPageXuatKho);
            tabControlMain.SelectedTab = tabPageXuatKho;
        }
    }
}
