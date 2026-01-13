using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class KhachHangBLL
    {
        private KhachHangDAL khachHangDAL = new KhachHangDAL();

        private const decimal RankBacMinExclusive = 500_000m;
        private const decimal RankVangMinExclusive = 2_000_000m;
        private const decimal RankBachKimMinExclusive = 5_000_000m;
        private const decimal RankKimCuongMinExclusive = 8_000_000m;
        private const decimal RankVipMinExclusive = 12_000_000m;

        public DataTable HienThiDanhSachKhachHang()
        {
            return khachHangDAL.LayDanhSachKhachHang();
        }

        public DataTable HienThiDanhSachKhachHangTheoTrangThai(string mode)
        {
            return khachHangDAL.LayDanhSachKhachHangTheoTrangThai(mode);
        }

        public DataTable HienThiDanhSachKhachHangTomTat()
        {
            return khachHangDAL.LayDanhSachKhachHangTomTat();
        }

        public DataTable HienThiDanhSachKhachHangTomTatTheoTrangThai(string mode)
        {
            return khachHangDAL.LayDanhSachKhachHangTomTatTheoTrangThai(mode);
        }

        public DataTable TimKhachHangTheoTen(string tenKH, string mode)
        {
            tenKH = ChuanHoaTenKhachHang(tenKH);
            if (string.IsNullOrWhiteSpace(tenKH)) return new DataTable();
            if (string.IsNullOrWhiteSpace(mode)) mode = "Active";
            return khachHangDAL.LayDanhSachKhachHangByTen(tenKH.Trim(), mode);
        }

        public string ChuanHoaTenKhachHang(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return "";

            string s = ten.Trim();

            while (s.Contains("  ")) s = s.Replace("  ", " ");

            try
            {
                var culture = new CultureInfo("vi-VN");
                var words = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.ToLower(culture))
                    .Select(w =>
                    {
                        if (string.IsNullOrEmpty(w)) return w;
                        if (w.Length == 1) return w.ToUpper(culture);
                        return char.ToUpper(w[0], culture) + w.Substring(1);
                    });
                return string.Join(" ", words);
            }
            catch
            {
                return s;
            }
        }

        public KhachHang LayThongTinKhachHangBySDT(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return null;
            return khachHangDAL.LayThongTinKhachHangBySDT(sdt);
        }

        public bool ThemKhachHang(string tenKH, string sdt, string diaChi)
        {
            tenKH = ChuanHoaTenKhachHang(tenKH);
            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }
            KhachHang kh = new KhachHang { TenKH = tenKH, SDT = sdt, DiaChi = diaChi, TrangThai = "Active" };
            return khachHangDAL.ThemKhachHang(kh);
        }

        public bool CapNhatKhachHang(string tenKH, string sdt, string diaChi)
        {
            tenKH = ChuanHoaTenKhachHang(tenKH);
            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }
            KhachHang kh = new KhachHang { TenKH = tenKH, SDT = sdt, DiaChi = diaChi };
            return khachHangDAL.CapNhatKhachHang(kh);
        }

        public bool CapNhatKhachHangTheoSdtCu(string tenKH, string sdtMoi, string diaChi, string sdtCu)
        {
            tenKH = ChuanHoaTenKhachHang(tenKH);
            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdtMoi) || string.IsNullOrWhiteSpace(sdtCu))
            {
                return false;
            }
            KhachHang kh = new KhachHang { TenKH = tenKH, SDT = sdtMoi, DiaChi = diaChi };
            return khachHangDAL.CapNhatKhachHangTheoSdtCu(kh, sdtCu);
        }

        public bool XoaKhachHang(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }

            try
            {
                return khachHangDAL.CapNhatTrangThaiKhachHang(sdt, "Inactive");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CapNhatTrangThaiKhachHang(string sdt, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return false;
            if (string.IsNullOrWhiteSpace(trangThai)) return false;
            return khachHangDAL.CapNhatTrangThaiKhachHang(sdt, trangThai);
        }

        public decimal LayTongChiTieuBySDT(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return 0m;
            return khachHangDAL.LayTongChiTieuBySDT(sdt);
        }

        public string TinhRank(decimal tongChiTieu)
        {
            // Đồng bộ với frmSales.GetRankByTotalSpent
            if (tongChiTieu >= RankVipMinExclusive) return "VIP";
            if (tongChiTieu >= RankKimCuongMinExclusive) return "Kim Cương";
            if (tongChiTieu >= RankBachKimMinExclusive) return "Bạch Kim";
            if (tongChiTieu >= RankVangMinExclusive) return "Vàng";
            if (tongChiTieu >= RankBacMinExclusive) return "Bạc";
            return "Thành Viên";
        }

        public string LayRankBySDT(string sdt)
        {
            decimal tongChiTieu = LayTongChiTieuBySDT(sdt);
            return TinhRank(tongChiTieu);
        }
    }
}
