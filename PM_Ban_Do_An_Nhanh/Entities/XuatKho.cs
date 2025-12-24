using System;

namespace PM_Ban_Do_An_Nhanh.Entities
{
    public class PhieuXuatKho
    {
        public int MaPX { get; set; }
        public DateTime NgayXuat { get; set; }
        public int? MaTK { get; set; }
        public string LyDo { get; set; }
        public string GhiChu { get; set; }
    }

    public class ChiTietPhieuXuatKho
    {
        public int MaPX { get; set; }
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public int SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? ThanhTien => DonGia.HasValue ? SoLuong * DonGia.Value : (decimal?)null;
    }
}
