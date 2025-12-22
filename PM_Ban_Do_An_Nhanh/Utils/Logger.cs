using System;
using System.IO;

namespace PM_Ban_Do_An_Nhanh.Utils
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);
                    string file = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
                    File.AppendAllText(file, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // swallow - logging should not crash app
            }
        }

        public static void Log(Exception ex)
        {
            if (ex == null) return;
            Log(ex.ToString());
        }
    }
}
