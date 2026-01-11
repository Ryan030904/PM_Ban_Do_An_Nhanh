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
        private TabPage tabPageKhuyenMai;
        private bool isHandlingTabSelection;

        private static string FixVietnameseMojibake(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            try
            {
                // Common case: UTF-8 bytes were interpreted as Latin1/Windows-1252 -> shows as "Quá°£n trá»‹"
                var latin1 = Encoding.GetEncoding(28591); // ISO-8859-1
                var bytes = latin1.GetBytes(input);
                var candidate = Encoding.UTF8.GetString(bytes);

                // Heuristic: if candidate contains Vietnamese letters and original looks like mojibake
                if (ContainsVietnameseLetters(candidate) && !ContainsVietnameseLetters(input))
                    return candidate;

                // Also accept candidate if it removes typical mojibake symbols
                if (candidate.IndexOfAny(new[] { '°', '»', '¼', '½', '¾', '¿', 'Ã', 'Â', 'á' }) < 0 &&
                    input.IndexOfAny(new[] { '°', '»', '¼', '½', '¾', '¿', 'Ã', 'Â', 'á' }) >= 0)
                    return candidate;
            }
            catch
            {
            }

            return input;
        }

        private static bool ContainsVietnameseLetters(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var ch in s)
            {
                if ("ăâđêôơưáàảãạéèẻẽẹíìỉĩịóòỏõọúùủũụýỳỷỹỵĂÂĐÊÔƠƯÁÀẢÃẠÉÈẺẼẸÍÌỈĨỊÓÒỎÕỌÚÙỦŨỤÝỲỶỸỴ".IndexOf(ch) >= 0)
                    return true;
            }
            return false;
        }

        public frmMain()
        {
            InitializeComponent();
            this.Text = "Hệ thống quản lý bán thức ăn nhanh";
            HienThiThongTinNguoiDung();
            // Do not create the tab page here — create it lazily when needed to avoid
            // possible duplication or issues if designer changes tabControl.

            tabControlMain.SelectedIndexChanged += tabControlMain_SelectedIndexChanged;
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isHandlingTabSelection) return;
            if (tabControlMain == null) return;
            if (tabControlMain.SelectedTab == null) return;

            isHandlingTabSelection = true;
            try
            {
                var selected = tabControlMain.SelectedTab;

                if (selected == tabPageSales)
                {
                    LoadFormInTabPage(new frmSales(), ref tabPageSales, tabPageSales.Text);
                }
                else if (selected == tabPageCustomer)
                {
                    LoadFormInTabPage(new frmAddCustomer("", false, true), ref tabPageCustomer, tabPageCustomer.Text);
                }
                else if (selected == tabPageMenu)
                {
                    LoadFormInTabPage(new frmMenuManagement(), ref tabPageMenu, tabPageMenu.Text);
                }
                else if (selected == tabPageReport)
                {
                    LoadFormInTabPage(new frmReport(), ref tabPageReport, tabPageReport.Text);
                }
                else if (selected.Text == "Danh Mục")
                {
                    // If user selects Danh Mục tab from the top, ensure content is loaded.
                    if (tabPageDanhMuc == null) tabPageDanhMuc = selected;
                    LoadFormInTabPage(new frmDanhMuc(), ref tabPageDanhMuc, "Danh Mục");
                }
                else if (selected.Text == "Khuyến mãi")
                {
                    if (tabPageKhuyenMai == null) tabPageKhuyenMai = selected;
                    LoadFormInTabPage(new frmKhuyenMai(), ref tabPageKhuyenMai, "Khuyến mãi");
                }
            }
            finally
            {
                isHandlingTabSelection = false;
            }
        }

        private void HienThiThongTinNguoiDung()
        {
            if (GlobalVariables.LoggedInUser != null)
            {
                var ten = FixVietnameseMojibake(GlobalVariables.LoggedInUser.TenTK);
                lblUserInfo.Text = $"Xin chào, {ten}";
                btnSales.Enabled = true;
                btnMenuManagement.Enabled = true;
                btnReport.Enabled = true;
            }
            else
            {
                MessageBox.Show("Phiên làm việc hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Close this main form if no user is logged in — caller (Program) should show login flow.
                this.Close();
            }
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            frmSales salesForm = new frmSales();
            LoadFormInTabPage(salesForm, ref tabPageSales, "Bán hàng");
            tabControlMain.SelectedTab = tabPageSales;
        }

        private void btnKhachHang_Click(object sender, EventArgs e)
        {
            // Main customer screen: searchable management (search + edit/delete)
            frmAddCustomer khachHangForm = new frmAddCustomer("", false, true);
            LoadFormInTabPage(khachHangForm, ref tabPageCustomer, "Khách hàng");
            tabControlMain.SelectedTab = tabPageCustomer;
        }

        private void btnMenuManagement_Click(object sender, EventArgs e)
        {
            frmMenuManagement menuForm = new frmMenuManagement();
            LoadFormInTabPage(menuForm, ref tabPageMenu, "Quản lý món");
            tabControlMain.SelectedTab = tabPageMenu;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            frmReport reportForm = new frmReport();
            LoadFormInTabPage(reportForm, ref tabPageReport, "Báo cáo");
            tabControlMain.SelectedTab = tabPageReport;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống không?", "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Hide main and show login dialog. If login succeeds, restore; otherwise exit.
                this.Hide();
                using (var loginForm = new frmLogin())
                {
                    var result = loginForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // A new user logged in — refresh UI and show main again
                        HienThiThongTinNguoiDung();
                        this.Show();
                    }
                    else
                    {
                        // No login — exit application
                        Application.Exit();
                    }
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // Helper to load a child form into a named tab page. Creates the tab page lazily.
        private void LoadFormInTabPage(Form childForm, ref TabPage tabPageField, string tabTitle)
        {
            if (tabControlMain == null) return;

            if (tabPageField == null)
            {
                tabPageField = new TabPage(tabTitle);
                // assign a unique name to avoid duplicates
                tabPageField.Name = tabTitle.Replace(" ", "_");
                tabControlMain.TabPages.Add(tabPageField);
            }

            // If tab already contains a control of the same form type, just select it
            var existing = tabPageField.Controls.OfType<Form>().FirstOrDefault();
            if (existing != null)
            {
                // If it's the same type, bring to front
                if (existing.GetType() == childForm.GetType())
                {
                    tabControlMain.SelectedTab = tabPageField;
                    childForm.Close();
                    childForm.Dispose();
                    return;
                }
                else
                {
                    // Dispose any previous form in this tab
                    existing.Close();
                    existing.Dispose();
                    tabPageField.Controls.Clear();
                }
            }

            // Embed the form
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            tabPageField.Controls.Add(childForm);
            childForm.Show();
        }

        private void btnDanhMuc_Click(object sender, EventArgs e)
        {
            // Create and add the Danh Mục tab lazily to avoid duplicate tabs or
            // possible ordering/initialization issues.
            if (tabPageDanhMuc == null)
            {
                tabPageDanhMuc = new TabPage("Danh Mục");
                tabControlMain.TabPages.Add(tabPageDanhMuc);
            }

            frmDanhMuc danhmucForm = new frmDanhMuc();
            LoadFormInTabPage(danhmucForm, ref tabPageDanhMuc, "Danh Mục");
            tabControlMain.SelectedTab = tabPageDanhMuc;
        }

        private void btnKhuyenMai_Click(object sender, EventArgs e)
        {
            if (tabPageKhuyenMai == null)
            {
                tabPageKhuyenMai = new TabPage("Khuyến mãi");
                tabControlMain.TabPages.Add(tabPageKhuyenMai);
            }

            frmKhuyenMai khuyenMaiForm = new frmKhuyenMai();
            LoadFormInTabPage(khuyenMaiForm, ref tabPageKhuyenMai, "Khuyến mãi");
            tabControlMain.SelectedTab = tabPageKhuyenMai;
        }
    }
}
