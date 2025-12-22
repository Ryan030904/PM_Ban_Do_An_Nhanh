using System.Data.SqlClient;
using PM_Ban_Do_An_Nhanh.DAL;

namespace PM_Ban_Do_An_Nhanh
{
    // Facade in root namespace that delegates to DAL implementation.
    public static class DBConnection
    {
        private static readonly IDBConnection _impl = new DefaultDbConnection();

        public static SqlConnection GetConnection()
        {
            return _impl.GetConnection();
        }

        public static bool TestConnection(out string errorMessage)
        {
            return _impl.TestConnection(out errorMessage);
        }
    }
}