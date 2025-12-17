using System;
using System.Data;
using PM_Ban_Do_An_Nhanh.DAL;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class DanhMucBLL
    {
        private DanhMucDAL danhMucDAL = new DanhMucDAL();

        public DataTable LayDanhSachDanhMuc()
        {
            return danhMucDAL.LayDanhSachDanhMuc();
        }

        public bool ThemDanhMuc(string tenDM)
        {
            if (string.IsNullOrWhiteSpace(tenDM))
                throw new ArgumentException("Tên danh mục không được để trống.");

            return danhMucDAL.ThemDanhMuc(tenDM);
        }

        public bool SuaDanhMuc(int maDM, string tenDM)
        {
            if (maDM <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ.");

            if (string.IsNullOrWhiteSpace(tenDM))
                throw new ArgumentException("Tên danh mục không được để trống.");

            return danhMucDAL.SuaDanhMuc(maDM, tenDM);
        }

        public bool XoaDanhMuc(int maDM)
        {
            if (maDM <= 0)
                throw new ArgumentException("Mã danh mục không hợp lệ.");

            return danhMucDAL.XoaDanhMuc(maDM);
        }
    }
}