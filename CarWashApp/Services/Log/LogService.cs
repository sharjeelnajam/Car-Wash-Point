namespace CarWashApp.Services.Log
{
    public static class LogService
    {
        public static void Log(string text)
        {
            try
            {
                string logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, "scanner-test.log");
                File.AppendAllText(logPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {text}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                throw;// You may want to log the exception to Event Viewer
            }
        }
    }
}
