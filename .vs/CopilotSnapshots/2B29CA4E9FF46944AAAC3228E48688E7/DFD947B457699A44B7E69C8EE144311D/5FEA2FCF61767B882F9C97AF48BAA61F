using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;
using PM_Ban_Do_An_Nhanh.Helpers;
using PM_Ban_Do_An_Nhanh.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class MonAnBLL
    {
        private MonAnDAL monAnDAL = new MonAnDAL();

        public DataTable HienThiDanhSachMonAn()
        {
            try
            {
                return monAnDAL.LayDanhSachMonAn();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                // Return empty table to avoid null checks everywhere
                return new DataTable();
            }
        }

        public bool ThemMonAn(string tenMon, decimal gia, int maDM, string trangThai, string imageSourcePath = null)
        {
            try
            {
                MonAn monAn = new MonAn
                {
                    TenMon = tenMon,
                    Gia = gia,
                    MaDM = maDM,
                    TrangThai = trangThai
                };

                // Thêm món ăn trước để lấy ID
                int newMonAnId = monAnDAL.ThemMonAn(monAn);
                if (newMonAnId > 0)
                {
                    // Lưu ảnh nếu có
                    if (!string.IsNullOrEmpty(imageSourcePath))
                    {
                        string imagePath = ImageHelper.SaveMenuItemImage(imageSourcePath, newMonAnId, tenMon);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            // Cập nhật đường dẫn ảnh
                            monAnDAL.CapNhatDuongDanAnh(newMonAnId, imagePath);
                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public bool SuaMonAn(int maMon, string tenMon, decimal gia, int maDM, string trangThai, string imageSourcePath = null)
        {
            try
            {
                // Lấy thông tin ảnh cũ để xóa nếu cần
                string oldImagePath = monAnDAL.LayDuongDanAnhByMaMon(maMon);

                MonAn monAn = new MonAn
                {
                    MaMon = maMon,
                    TenMon = tenMon,
                    Gia = gia,
                    MaDM = maDM,
                    TrangThai = trangThai
                };

                // Xử lý ảnh mới nếu có
                if (!string.IsNullOrEmpty(imageSourcePath))
                {
                    string newImagePath = ImageHelper.SaveMenuItemImage(imageSourcePath, maMon, tenMon);
                    monAn.HinhAnh = newImagePath;

                    // Xóa ảnh cũ nếu khác ảnh mới
                    if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != newImagePath)
                    {
                        ImageHelper.DeleteMenuItemImage(oldImagePath);
                    }
                }
                else
                {
                    // Giữ nguyên ảnh cũ
                    monAn.HinhAnh = oldImagePath;
                }

                return monAnDAL.SuaMonAn(monAn);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public bool XoaMonAn(int maMon)
        {
            if (maMon <= 0) return false;

            try
            {
                // Lấy đường dẫn ảnh để xóa
                string imagePath = monAnDAL.LayDuongDanAnhByMaMon(maMon);

                // Xóa món ăn từ database
                bool result = monAnDAL.XoaMonAn(maMon);

                // Xóa ảnh nếu xóa thành công
                if (result && !string.IsNullOrEmpty(imagePath))
                {
                    ImageHelper.DeleteMenuItemImage(imagePath);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }
    }
}

// Connection string for the database
// Data Source=.\SQLEXPRESS;Initial Catalog=FastFoodDB;User ID=sa;Password=your_password;TrustServerCertificate=True
