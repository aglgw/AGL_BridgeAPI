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
        /// <summary>
        /// 일반유틸로그(티타임수집)
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        public static void LogReg(string projectName, string logName, string logTitle, string logTxt, bool isError = false)
        {
            try
            {
                DateTime kstTime = DateTime.UtcNow.AddHours(9);

                string fullPath = Path.Combine(logBasePath + projectName, logName);
                string fileName = Path.Combine(fullPath, $"{kstTime:yyyyMMddHH}.log");

                Directory.CreateDirectory(fullPath);

                lock (lockObject)
                {
                    using (StreamWriter sw = File.AppendText(fileName))
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
                    logName += "\\Error";
                    LogReg(projectName, logName, logTitle, logTxt, false);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리는 필요한 경우에 맞게 수정해주세요.
                LogService.logInformation($"로그실패[LogReg]({logName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 로그(예약/예약취소)
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="fileName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        public static void ReserveLogReg(string pathName, string projectName, string fileName, string logTitle, string logTxt, bool isError = false)
        {
            try
            {
                DateTime kstTime = DateTime.UtcNow.AddHours(9);

                string fullPath = Path.Combine(logBasePath + projectName, pathName);
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
                    fileName += "\\Error";
                    ReserveLogReg(pathName, projectName, fileName, logTitle, logTxt, false);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리는 필요한 경우에 맞게 수정해주세요.
                LogService.logInformation($"로그실패[LogReg]({fileName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 로그삭제
        /// </summary>
        /// <param name="logName"></param>
        public static void LogDelete(string projectName, string logName, int limitDays)
        {
            string directoryPath = Path.Combine(logBasePath + projectName, logName);
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