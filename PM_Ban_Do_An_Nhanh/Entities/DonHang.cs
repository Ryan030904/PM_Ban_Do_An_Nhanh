using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.Entities
{
    public class DonHang
    {
        public int MaDH { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public int? MaKH { get; set; }
        public string TenKhachHang { get; set; }
        public string SDTKhachHang { get; set; }
    }
}
