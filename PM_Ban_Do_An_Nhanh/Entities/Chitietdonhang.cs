using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.Entities
{
    public class ChiTietDonHang
    {

        public int MaDH { get; set; }
        public int MaMon { get; set; }
        public string TenMon { get; set; } 
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
