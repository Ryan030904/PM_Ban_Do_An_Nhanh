using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PM_Ban_Do_An_Nhanh.Utils;

namespace PM_Ban_Do_An_Nhanh
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // seed sample images if needed
            ImageSeeder.SeedIfNeeded();

            // Global exception handlers to catch silent crashes on startup
            Application.ThreadException += (sender, e) =>
            {
                try
                {
                    MessageBox.Show("Unhandled UI exception:\n" + e.Exception.Message + "\n" + e.Exception.StackTrace,
                        "Lỗi ứng dụng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    var ex = e.ExceptionObject as Exception;
                    MessageBox.Show("Unhandled non-UI exception:\n" + (ex?.Message ?? e.ExceptionObject.ToString()) + "\n" + (ex?.StackTrace ?? ""),
                        "Lỗi ứng dụng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            };

            // Show login as a modal dialog first. Only run main form when login succeeds.
            using (var loginForm = new frmLogin())
            {
                var result = loginForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        Application.Run(new frmMain());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi khởi chạy form chính:\n" + ex.Message + "\n" + ex.StackTrace,
                            "Lỗi ứng dụng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // else: exit application
            }
        }
    }
}
