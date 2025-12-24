using System;

namespace PM_Ban_Do_An_Nhanh.Entities
{
    public class PhieuNhapKho
    {
        public int MaPN { get; set; }
        public DateTime NgayNhap { get; set; }
        public int? MaTK { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTien { get; set; }
    }

    public class ChiTietPhieuNhapKho
    {
        public int MaPN { get; set; }
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
