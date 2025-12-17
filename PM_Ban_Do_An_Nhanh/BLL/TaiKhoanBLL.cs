using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class TaiKhoanBLL
    {
        private TaiKhoanDAL taiKhoanDAL = new TaiKhoanDAL();

        public TaiKhoan KiemTraDangNhap(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return null;
            }
            return taiKhoanDAL.KiemTraDangNhap(tenDangNhap, matKhau);
        }
    }
}