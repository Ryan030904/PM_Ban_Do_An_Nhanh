using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class KhachHangBLL
    {
        private KhachHangDAL khachHangDAL = new KhachHangDAL();

        public DataTable HienThiDanhSachKhachHang()
        {
            return khachHangDAL.LayDanhSachKhachHang();
        }

        public KhachHang LayThongTinKhachHangBySDT(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt)) return null;
            return khachHangDAL.LayThongTinKhachHangBySDT(sdt);
        }

        public bool ThemKhachHang(string tenKH, string sdt, string diaChi, string email, DateTime? ngaySinh)
        {
            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }
            KhachHang kh = new KhachHang { TenKH = tenKH, SDT = sdt, DiaChi = diaChi, Email = email, NgaySinh = ngaySinh };
            return khachHangDAL.ThemKhachHang(kh);
        }

        public bool CapNhatKhachHang(string tenKH, string sdt, string diaChi, string email, DateTime? ngaySinh)
        {
            if (string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }
            KhachHang kh = new KhachHang { TenKH = tenKH, SDT = sdt, DiaChi = diaChi, Email = email, NgaySinh = ngaySinh };
            return khachHangDAL.CapNhatKhachHang(kh);
        }

        public bool XoaKhachHang(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt))
            {
                return false;
            }

            try
            {
                return khachHangDAL.XoaKhachHang(sdt);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
