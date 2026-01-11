using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.Entities
{
    public class MonAn
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public decimal Gia { get; set; }
        public int MaDM { get; set; }
        public string TrangThai { get; set; }
        public string TenDanhMuc { get; set; }
        public string HinhAnh { get; set; } // Thay đổi từ byte[] thành string để lưu đường dẫn
        public int SoLuongTon { get; set; }
    }
}