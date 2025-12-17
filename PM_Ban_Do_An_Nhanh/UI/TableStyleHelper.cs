using System;
using System.Drawing;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh.UI
{
    public static class TableStyleHelper
    {
        public static void ApplyModernStyle(DataGridView grid, string theme = "primary")
        {
            Color headerColor, accentColor;
            
            switch (theme)
            {
                case "success":
                    headerColor = Color.FromArgb(46, 204, 113);
                    accentColor = Color.FromArgb(26, 188, 156);
                    break;
                case "warning":
                    headerColor = Color.FromArgb(230, 126, 34);
                    accentColor = Color.FromArgb(243, 156, 18);
                    break;
                case "danger":
                    headerColor = Color.FromArgb(231, 76, 60);
                    accentColor = Color.FromArgb(192, 57, 43);
                    break;
                default:
                    headerColor = Color.FromArgb(52, 152, 219);
                    accentColor = Color.FromArgb(41, 128, 185);
                    break;
            }

            // Basic grid properties
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.RowHeadersVisible = false;
            grid.ColumnHeadersHeight = 45;
            grid.RowTemplate.Height = 35;
            grid.GridColor = Color.FromArgb(230, 230, 230);
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

            // Header style
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = headerColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = headerColor,
                Padding = new Padding(8, 8, 8, 8)
            };

            // Cell style
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 10F),
                SelectionBackColor = Color.FromArgb(174, 214, 241),
                SelectionForeColor = Color.FromArgb(44, 62, 80),
                Padding = new Padding(8, 4, 8, 4)
            };

            // Alternating row style
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                SelectionBackColor = Color.FromArgb(174, 214, 241)
            };

            // Add hover effect
            grid.CellMouseEnter += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }
            };

            grid.CellMouseLeave += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = 
                        e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 249, 250);
                }
            };
        }

        public static void AddEmptyStateMessage(DataGridView grid, string message = "Không có dữ liệu để hiển thị")
        {
            if (grid.Rows.Count == 0)
            {
                // Create empty state overlay
                Panel emptyPanel = new Panel
                {
                    Size = grid.Size,
                    BackColor = Color.White,
                    Location = grid.Location
                };

                Label emptyLabel = new Label
                {
                    Text = message,
                    Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(149, 165, 166),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                emptyPanel.Controls.Add(emptyLabel);
                grid.Parent.Controls.Add(emptyPanel);
                emptyPanel.BringToFront();
            }
        }
    }
}