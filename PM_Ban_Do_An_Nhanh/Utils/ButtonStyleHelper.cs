using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh.Utils
{
    public static class ButtonStyleHelper
    {
        // Định nghĩa các màu chuẩn
        public static readonly Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        public static readonly Color SuccessGreen = Color.FromArgb(46, 204, 113);
        public static readonly Color DangerRed = Color.FromArgb(231, 76, 60);
        public static readonly Color WarningOrange = Color.FromArgb(230, 126, 34);
        public static readonly Color InfoCyan = Color.FromArgb(26, 188, 156);
        public static readonly Color SecondaryGray = Color.FromArgb(127, 140, 141);

        // Định nghĩa kích thước button chuẩn
        public static readonly Size SmallButtonSize = new Size(80, 35);
        public static readonly Size MediumButtonSize = new Size(120, 40);
        public static readonly Size LargeButtonSize = new Size(160, 45);

        // Phương thức style button chuẩn
        public static void ApplyModernStyle(Button btn, Color backgroundColor, string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.BackColor = backgroundColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", GetFontSizeByButtonSize(size), FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Size = GetSizeByButtonSize(size);

            // Rounded corners effect using Region property
            btn.Region = new Region(CreateRoundedRectanglePath(new Rectangle(0, 0, btn.Width, btn.Height), 4));

            // Hover effects
            btn.MouseEnter += (s, e) => {
                btn.BackColor = LightenColor(backgroundColor, 20);
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = backgroundColor;
            };

            // Click effect
            btn.MouseDown += (s, e) => {
                btn.BackColor = DarkenColor(backgroundColor, 15);
            };
            btn.MouseUp += (s, e) => {
                btn.BackColor = backgroundColor;
            };

            if (!string.IsNullOrEmpty(tooltip))
            {
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(btn, tooltip);
            }
        }

        // Các phương thức tiện ích cho từng loại button
        public static void ApplyPrimaryStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, PrimaryBlue, tooltip, size);
        }

        public static void ApplySuccessStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, SuccessGreen, tooltip, size);
        }

        public static void ApplyDangerStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, DangerRed, tooltip, size);
        }

        public static void ApplyWarningStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, WarningOrange, tooltip, size);
        }

        public static void ApplyInfoStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, InfoCyan, tooltip, size);
        }

        public static void ApplySecondaryStyle(Button btn, string text = "", string tooltip = "", ButtonSize size = ButtonSize.Medium)
        {
            btn.Text = text;
            ApplyModernStyle(btn, SecondaryGray, tooltip, size);
        }

        // Phương thức làm sáng màu
        public static Color LightenColor(Color color, int amount)
        {
            int red = Math.Min(255, color.R + amount);
            int green = Math.Min(255, color.G + amount);
            int blue = Math.Min(255, color.B + amount);
            return Color.FromArgb(red, green, blue);
        }

        // Phương thức làm tối màu
        public static Color DarkenColor(Color color, int amount)
        {
            int red = Math.Max(0, color.R - amount);
            int green = Math.Max(0, color.G - amount);
            int blue = Math.Max(0, color.B - amount);
            return Color.FromArgb(red, green, blue);
        }

        // Helper methods cho size
        private static Size GetSizeByButtonSize(ButtonSize size)
        {
            switch (size)
            {
                case ButtonSize.Small:
                    return SmallButtonSize;
                case ButtonSize.Large:
                    return LargeButtonSize;
                default:
                    return MediumButtonSize;
            }
        }

        private static float GetFontSizeByButtonSize(ButtonSize size)
        {
            switch (size)
            {
                case ButtonSize.Small:
                    return 9F;
                case ButtonSize.Large:
                    return 11F;
                default:
                    return 10F;
            }
        }

        // Helper method to create a rounded rectangle path
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Top-left arc
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top-right arc
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom-right arc
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom-left arc
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // Style cho các control khác
        public static void ApplyModernTextBoxStyle(TextBox textBox)
        {
            textBox.Font = new Font("Segoe UI", 11F);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.ForeColor = Color.FromArgb(44, 62, 80);
        }

        public static void ApplyModernComboBoxStyle(ComboBox comboBox)
        {
            comboBox.Font = new Font("Segoe UI", 11F);
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = Color.FromArgb(44, 62, 80);
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        public static void ApplyModernDataGridViewStyle(DataGridView dgv, string theme = "primary")
        {
            Color headerColor, accentColor;

            switch (theme.ToLower())
            {
                case "success":
                    headerColor = SuccessGreen;
                    accentColor = InfoCyan;
                    break;
                case "warning":
                    headerColor = WarningOrange;
                    accentColor = Color.FromArgb(243, 156, 18);
                    break;
                case "danger":
                    headerColor = DangerRed;
                    accentColor = Color.FromArgb(192, 57, 43);
                    break;
                default:
                    headerColor = PrimaryBlue;
                    accentColor = Color.FromArgb(41, 128, 185);
                    break;
            }

            // Header style
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = headerColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = headerColor,
                Padding = new Padding(8, 8, 8, 8)
            };

            // Cell style
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 10F),
                SelectionBackColor = Color.FromArgb(174, 214, 241),
                SelectionForeColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(8, 4, 8, 4)
            };

            // Alternating row style
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                SelectionBackColor = Color.FromArgb(174, 214, 241)
            };

            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = Color.FromArgb(230, 230, 230);
            dgv.RowHeadersVisible = false;
            dgv.ColumnHeadersHeight = 45;
            dgv.RowTemplate.Height = 35;
        }
    }

    public enum ButtonSize
    {
        Small,
        Medium,
        Large
    }
}