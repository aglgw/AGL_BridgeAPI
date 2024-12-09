using Microsoft.Extensions.Configuration;
using Serilog;

namespace AGL.Api.ApplicationCore.Utilities
{
    public static class LogService
    {
        public static void InitLogger()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            //var logFilePath = configuration["Serilog:LogFilePath"];
            //Log.Logger = new LoggerConfiguration()
            //    //.MinimumLevel.Verbose()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            //    .ReadFrom.Configuration(configuration)
            //    .CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }


        public static void logVerbose(string message)
        {
            InitLogger();
            Log.Verbose(message);
            Log.CloseAndFlush();
        }

        public static void logDebug(string message)
        {
            InitLogger();
            Log.Debug(message);
            Log.CloseAndFlush();
        }

        public static void logInformation(string message)
        {
            InitLogger();
            Log.Information(message);
            Log.CloseAndFlush();
        }

        public static void logWarning(string message)
        {
            InitLogger();
            Log.Warning(message);
            Log.CloseAndFlush();
        }

        public static void logError(string message)
        {
            InitLogger();
            Log.Error(message);
            Log.CloseAndFlush();
        }

        public static void logFatal(string message)
        {
            InitLogger();
            Log.Fatal(message);
            Log.CloseAndFlush();
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public class Logs
    {
        private static readonly object lockObject = new object();
        private static readonly string logBasePath = @$"C:\AGL\Logs\";
        private static readonly string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
        /// <summary>
        /// 시간별로그
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        public static void LogRegHour(string projectName, string folderName, string fileName, string logTitle, string logTxt, bool isError = false)
        {
            try
            {
                DateTime kstTime = DateTime.UtcNow.AddHours(9);

                string fullPath = Path.Combine(logBasePath, projectName, environmentName, folderName);
                string logfileName = Path.Combine(fullPath, $"{fileName}_{kstTime:yyyyMMddHH}.log");

                Directory.CreateDirectory(fullPath);

                lock (lockObject)
                {
                    using (StreamWriter sw = File.AppendText(logfileName))
                    {
                        sw.WriteLine($"KST {kstTime:G} {logTitle}");
                        if (logTxt != "")
                        {
                            sw.WriteLine($"KST {kstTime:G} {logTxt}");
                        }

                    }
                }

                if (isError)
                {
                    folderName += "\\Error";
                    LogRegHour(projectName, folderName, fileName, logTitle, logTxt, false);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리는 필요한 경우에 맞게 수정해주세요.
                LogService.logInformation($"로그실패[LogRegHour]({folderName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 일자별로그
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        public static void LogRegDay(string projectName, string folderName, string fileName, string logTitle, string logTxt, bool isError = false)
        {
            try
            {
                DateTime kstTime = DateTime.UtcNow.AddHours(9);

                string fullPath = Path.Combine(logBasePath, projectName, environmentName, folderName);
                string logfileName = Path.Combine(fullPath, $"{fileName}_{kstTime:yyyyMMdd}.log");

                Directory.CreateDirectory(fullPath);

                lock (lockObject)
                {
                    using (StreamWriter sw = File.AppendText(logfileName))
                    {
                        sw.WriteLine($"KST {kstTime:G} {logTitle}");
                        if (logTxt != "")
                        {
                            sw.WriteLine($"KST {kstTime:G} {logTxt}");
                        }

                    }
                }

                if (isError)
                {
                    folderName += "\\Error";
                    LogRegDay(projectName, folderName, fileName, logTitle, logTxt, false);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리는 필요한 경우에 맞게 수정해주세요.
                LogService.logInformation($"로그실패[LogRegDay]({folderName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 로그삭제
        /// </summary>
        /// <param name="logName"></param>
        public static void LogDelete(string projectName, string logName, int limitDays)
        {
            string directoryPath = Path.Combine(logBasePath , projectName, environmentName, logName);
            try
            {
                var logFiles = Directory.GetFiles(directoryPath, "*.log", SearchOption.AllDirectories);
                DateTime now = DateTime.Now;
                LogService.logInformation($"LogDelete Start directoryPath: {directoryPath}, logFiles: {logFiles.Count()}");
                foreach (var logFile in logFiles)
                {
                    try
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(logFile);
                        if ((now - lastWriteTime).TotalDays > limitDays)
                        {
                            File.Delete(logFile);
                            LogService.logInformation($"Deleted: {logFile}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.logInformation($"Error deleting file {logFile}: {ex.Message}");
                    }
                }
                LogService.logInformation($"로그삭제[LogDelete]({directoryPath})");
            }
            catch (Exception ex)
            {
                // 예외가 발생할 경우, 예외 메시지를 출력합니다.
                LogService.logInformation($"로그삭제실패[LogDelete]({directoryPath}):{ex.Message}");
            }
        }

    }
}