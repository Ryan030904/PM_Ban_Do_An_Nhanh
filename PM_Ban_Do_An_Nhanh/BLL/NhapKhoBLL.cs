using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class NhapKhoBLL
    {
        private readonly NhapKhoDAL nhapKhoDAL = new NhapKhoDAL();

        public int TaoPhieuNhap(string ghiChu, List<ChiTietPhieuNhapKho> chiTietList)
        {
            if (chiTietList == null || chiTietList.Count == 0)
                throw new ArgumentException("Danh sách chi tiết nhập kho không được trống");

            if (chiTietList.Any(x => x.MaMon <= 0 || x.SoLuong <= 0 || x.DonGia <= 0))
                throw new ArgumentException("Chi tiết nhập kho không hợp lệ");

            var phieu = new PhieuNhapKho
            {
                NgayNhap = DateTime.Now,
                MaTK = GlobalVariables.LoggedInUser != null ? (int?)GlobalVariables.LoggedInUser.MaTK : null,
                GhiChu = ghiChu,
                TongTien = chiTietList.Sum(x => x.ThanhTien)
            };

            return nhapKhoDAL.ThemPhieuNhap(phieu, chiTietList);
        }

        public DataTable LayDanhSachPhieuNhap(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return nhapKhoDAL.LayDanhSachPhieuNhap(tuNgay, denNgay);
        }

        public DataTable LayChiTietPhieuNhap(int maPN)
        {
            return nhapKhoDAL.LayChiTietPhieuNhap(maPN);
        }
    }
}
