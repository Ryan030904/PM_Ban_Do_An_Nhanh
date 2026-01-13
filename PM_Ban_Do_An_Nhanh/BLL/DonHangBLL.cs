using System;
using System.Data;
using System.Collections.Generic;
using PM_Ban_Do_An_Nhanh.DAL;
using PM_Ban_Do_An_Nhanh.Entities;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class DonHangBLL
    {
        private DonHangDAL donHangDAL = new DonHangDAL();
        private static DateTime lastOrderTime = DateTime.MinValue;
        private static readonly object lockObject = new object();

        public int ThemDonHang(DonHang donHang, List<ChiTietDonHang> chiTietList)
        {
            lock (lockObject)
            {
                // Enhanced validation
                if (donHang == null)
                    throw new ArgumentNullException(nameof(donHang));

                if (donHang.TongTien <= 0)
                    throw new ArgumentException("Tổng tiền phải lớn hơn 0");

                if (chiTietList == null || chiTietList.Count == 0)
                    throw new ArgumentException("Danh sách chi tiết đơn hàng không được trống");

                // Prevent rapid fire submissions
                TimeSpan timeSinceLastOrder = DateTime.Now - lastOrderTime;
                if (timeSinceLastOrder.TotalSeconds < 2)
                {
                    throw new InvalidOperationException("Vui lòng đợi ít nhất 2 giây trước khi tạo đơn hàng mới.");
                }

                // Validate each order item
                foreach (var item in chiTietList)
                {
                    if (item.MaMon <= 0)
                        throw new ArgumentException($"Mã món không hợp lệ: {item.MaMon}");

                    if (item.SoLuong <= 0)
                        throw new ArgumentException($"Số lượng phải lớn hơn 0 cho món: {item.TenMon}");

                    if (item.DonGia <= 0)
                        throw new ArgumentException($"Đơn giá phải lớn hơn 0 cho món: {item.TenMon}");
                }

                try
                {
                    int result = donHangDAL.ThemDonHang(donHang, chiTietList);
                    if (result > 0)
                    {
                        lastOrderTime = DateTime.Now;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi thêm đơn hàng: {ex.Message}", ex);
                }
            }
        }

        public string XoaDonHangTrungLap()
        {
            try
            {
                int deletedCount = donHangDAL.XoaDonHangTrungLap();

                if (deletedCount >= 0)
                {
                    return $"Đã xóa thành công {deletedCount} đơn hàng trùng lặp.";
                }
                else
                {
                    return "Có lỗi xảy ra khi xóa đơn hàng trùng lặp.";
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        public DataTable LayChiTietDonHangChoIn(int maDH)
        {
            return donHangDAL.LayChiTietDonHangChoIn(maDH);
        }

        public DataTable LayDanhSachDonHang(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return donHangDAL.LayDanhSachDonHang(tuNgay, denNgay);
        }

        public DataTable LayThongKeDoanhThu(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return donHangDAL.LayThongKeDoanhThu(tuNgay, denNgay);
        }

        public DataTable LayMonAnBanChay(DateTime? tuNgay = null, DateTime? denNgay = null, int topN = 5)
        {
            return donHangDAL.LayMonAnBanChay(tuNgay, denNgay, topN);
        }

        public bool XoaDonHang(int maDH)
        {
            return donHangDAL.XoaDonHang(maDH);
        }

        public string XoaTatCaDonHang()
        {
            try
            {
                int deletedCount = donHangDAL.XoaTatCaDonHang();

                if (deletedCount >= 0)
                {
                    return "Đã xóa thành công tất cả đơn hàng.";
                }
                else
                {
                    return "Có lỗi xảy ra khi xóa tất cả đơn hàng.";
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }
    }
}