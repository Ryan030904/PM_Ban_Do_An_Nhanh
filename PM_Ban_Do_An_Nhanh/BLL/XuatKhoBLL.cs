using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class XuatKhoBLL
    {
        private readonly XuatKhoDAL xuatKhoDAL = new XuatKhoDAL();

        public int TaoPhieuXuat(string lyDo, string ghiChu, List<ChiTietPhieuXuatKho> chiTietList)
        {
            if (chiTietList == null || chiTietList.Count == 0)
                throw new ArgumentException("Danh sách chi tiết xuất kho không được trống");

            if (chiTietList.Any(x => x.MaMon <= 0 || x.SoLuong <= 0))
                throw new ArgumentException("Chi tiết xuất kho không hợp lệ");

            var phieu = new PhieuXuatKho
            {
                NgayXuat = DateTime.Now,
                MaTK = GlobalVariables.LoggedInUser != null ? (int?)GlobalVariables.LoggedInUser.MaTK : null,
                LyDo = lyDo,
                GhiChu = ghiChu
            };

            return xuatKhoDAL.ThemPhieuXuat(phieu, chiTietList);
        }

        public DataTable LayDanhSachPhieuXuat(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return xuatKhoDAL.LayDanhSachPhieuXuat(tuNgay, denNgay);
        }

        public DataTable LayChiTietPhieuXuat(int maPX)
        {
            return xuatKhoDAL.LayChiTietPhieuXuat(maPX);
        }
    }
}
