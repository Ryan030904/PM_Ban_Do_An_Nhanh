using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public partial class frmLogin : Form
    {
       
        private string connectionString =
            @"Data Source=MSI\SQLEXPRESS;
              Initial Catalog=FastFoodDB;
              Integrated Security=True;";

        public frmLogin()
        {
            InitializeComponent();
            this.Text = "Đăng nhập hệ thống";

            // test nhanh
            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT MaTK, TenTK, TenDangNhap
                           FROM TaiKhoan
                           WHERE TenDangNhap = @user AND MatKhau = @pass";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        GlobalVariables.LoggedInUser = new User()
                        {
                            MaTK = Convert.ToInt32(reader["MaTK"]),
                            TenTK = reader["TenTK"].ToString(),
                            TenDangNhap = reader["TenDangNhap"].ToString()
                        };

                        MessageBox.Show("Đăng nhập thành công!",
                                        "Thành công",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);

                        // Signal success to the caller (Program.Main) and close the dialog.
                        this.DialogResult = DialogResult.OK;
                        // Do not call Application.Run or show main here when using ShowDialog
                    }
                    else
                    {
                        MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu",
                                        "Lỗi",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL:\n" + ex.Message,
                                "Lỗi hệ thống",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}