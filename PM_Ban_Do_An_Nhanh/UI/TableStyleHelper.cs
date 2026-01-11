using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh.UI
{
    public static class TableStyleHelper
    {
        private static readonly ConditionalWeakTable<DataGridView, VndFormatConfig> _vndConfigs =
            new ConditionalWeakTable<DataGridView, VndFormatConfig>();

        private sealed class VndFormatConfig
        {
            public HashSet<string> Columns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public static string FormatVnd(decimal amount)
        {
            return string.Format("{0:N0} VNĐ", amount);
        }

        public static void ApplyVndFormatting(DataGridView grid, params string[] columnNames)
        {
            if (grid == null) return;

            var cfg = _vndConfigs.GetOrCreateValue(grid);

            cfg.Columns = new HashSet<string>(columnNames ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (var colName in cfg.Columns)
            {
                if (grid.Columns.Contains(colName))
                    grid.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            grid.CellFormatting -= Grid_CellFormattingVnd;
            grid.CellFormatting += Grid_CellFormattingVnd;

            DisableSorting(grid);
            grid.DataBindingComplete -= Grid_DataBindingCompleteDisableSorting;
            grid.DataBindingComplete += Grid_DataBindingCompleteDisableSorting;
        }

        private static void Grid_CellFormattingVnd(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!(sender is DataGridView grid)) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.Value == null) return;

            if (!_vndConfigs.TryGetValue(grid, out var cfg)) return;
            if (cfg.Columns == null || cfg.Columns.Count == 0) return;

            var colName = grid.Columns[e.ColumnIndex]?.Name;
            if (string.IsNullOrWhiteSpace(colName)) return;
            if (!cfg.Columns.Contains(colName)) return;

            try
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal amount))
                {
                    e.Value = FormatVnd(amount);
                    e.FormattingApplied = true;
                }
            }
            catch { }
        }

        public static void DisableSorting(DataGridView grid)
        {
            if (grid == null) return;
            if (grid.Columns == null) return;

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col == null) continue;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (col.HeaderCell != null)
                    col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }

        private static void Grid_DataBindingCompleteDisableSorting(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!(sender is DataGridView grid)) return;
            DisableSorting(grid);
        }

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
                SelectionBackColor = accentColor,
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 4, 8, 4)
            };

            // Alternating row style
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                SelectionBackColor = accentColor,
                SelectionForeColor = Color.White
            };

            // Add hover effect
            grid.CellMouseEnter += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    if (!grid.Rows[e.RowIndex].Selected)
                        grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }
            };

            grid.CellMouseLeave += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    if (!grid.Rows[e.RowIndex].Selected)
                    {
                        grid.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                            e.RowIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 249, 250);
                    }
                }
            };

            DisableSorting(grid);
            grid.DataBindingComplete -= Grid_DataBindingCompleteDisableSorting;
            grid.DataBindingComplete += Grid_DataBindingCompleteDisableSorting;
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