using System.Data;
using PM_Ban_Do_An_Nhanh.DAL;

namespace PM_Ban_Do_An_Nhanh.BLL
{
    public class TonKhoBLL
    {
        private readonly TonKhoDAL tonKhoDAL = new TonKhoDAL();

        public DataTable LayTonKhoMonAn()
        {
            return tonKhoDAL.LayTonKhoMonAn();
        }
    }
}
