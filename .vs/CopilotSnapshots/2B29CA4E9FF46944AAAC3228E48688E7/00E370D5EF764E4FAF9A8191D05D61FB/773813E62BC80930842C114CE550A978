using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// removed System.Configuration usage to avoid assembly reference issues

namespace PM_Ban_Do_An_Nhanh.DAL
{
    public static class DBConnection
    {
        // khai báo
        private static string connectionString;

        static DBConnection()
        {
            // Try to select a working connection string from multiple candidates.
            var candidates = new[]
            {
                // prefer App.config value if available (try to read, but don't crash if System.Configuration missing)
                null,
                // common developer machine instance used in frmLogin
                @"Data Source=MSI\SQLEXPRESS;Initial Catalog=FastFoodDB;Integrated Security=True;TrustServerCertificate=True",
                // fallback to local default
                @"Data Source=.\SQLEXPRESS;Initial Catalog=FastFoodDB;Integrated Security=True;TrustServerCertificate=True",
                @"Data Source=.;Initial Catalog=FastFoodDB;Integrated Security=True;TrustServerCertificate=True"
            };

            // Try to get connection string from App.config safely
            try
            {
                // Use reflection to avoid compile-time dependency on System.Configuration if not referenced
                var configManagerType = Type.GetType("System.Configuration.ConfigurationManager, System.Configuration");
                if (configManagerType != null)
                {
                    var csColl = configManagerType.GetProperty("ConnectionStrings").GetValue(null, null);
                    if (csColl != null)
                    {
                        var indexer = csColl.GetType().GetProperty("Item", new[] { typeof(string) });
                        var csObj = indexer.GetValue(csColl, new object[] { "PM_Ban_Do_An_Nhanh_DB" });
                        if (csObj != null)
                        {
                            var connStrProp = csObj.GetType().GetProperty("ConnectionString");
                            var csValue = connStrProp.GetValue(csObj, null) as string;
                            if (!string.IsNullOrWhiteSpace(csValue))
                            {
                                candidates[0] = csValue;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore and continue with other candidates
            }

            foreach (var candidate in candidates.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                try
                {
                    using (var conn = new SqlConnection(candidate))
                    {
                        conn.Open();
                        // success
                        connectionString = candidate;
                        return;
                    }
                }
                catch
                {
                    // try next
                }
            }

            // If none worked, fall back to a sensible default (may still fail at runtime)
            connectionString = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=FastFoodDB;Integrated Security=True;TrustServerCertificate=True";
        }

        public static System.Data.SqlClient.SqlConnection GetConnection()
        {
            return new System.Data.SqlClient.SqlConnection(connectionString);
        }

        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}