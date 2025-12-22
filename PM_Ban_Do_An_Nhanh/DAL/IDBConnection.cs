using System.Data.SqlClient;

namespace PM_Ban_Do_An_Nhanh.DAL
{

    public interface IDBConnection
    {
        SqlConnection GetConnection();
        bool TestConnection(out string errorMessage);
    }
}