using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PM_Ban_Do_An_Nhanh
{
    public class NoScrollComboBox : ComboBox
    {
        private const int WM_MOUSEWHEEL = 0x020A;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                // Allow scrolling only when dropdown is open.
                if (!DroppedDown)
                {
                    try
                    {
                        // Forward mouse wheel to parent so the page/panel can scroll.
                        Control target = Parent;
                        if (target != null)
                        {
                            SendMessage(target.Handle, m.Msg, m.WParam, m.LParam);
                        }
                    }
                    catch { }

                    return; // swallow wheel -> prevent changing SelectedIndex
                }
            }

            base.WndProc(ref m);
        }
    }
}
