using AGL.Api.ApplicationCore.Utilities;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AGL.Api.API_Template.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class UtilLogs
    {
        /// <summary>
        /// 시간별로그
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="member"></param>
        public static void LogRegHour(string folderName, string fileName, string logTitle, string logTxt, bool isError = false, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            try
            {
                logTitle = $"File: {file} (Line: {line}) {member} : {logTitle}";
                Logs.LogRegHour(GetProjectName(), folderName, fileName, logTitle, logTxt, isError);
            }
            catch (Exception ex)
            {
                LogService.logInformation($"로그실패[LogRegHour]({folderName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 일자별로그
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="member"></param>
        public static void LogRegDay(string folderName, string fileName, string logTitle, string logTxt, bool isError = false, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            try
            {
                logTitle = $"File: {file} (Line: {line}) {member} : {logTitle}";
                Logs.LogRegDay(GetProjectName(), folderName, fileName, logTitle, logTxt, isError);
            }
            catch (Exception ex)
            {
                LogService.logInformation($"로그실패[LogRegDay]({fileName}):{ex.Message}");
            }
        }

        /// <summary>
        /// 로그삭제
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="limitDays"></param>
        public static void LogDelete(string logName, int limitDays = 30)
        {
            try
            {
                Logs.LogDelete(GetProjectName(), logName, limitDays);
                LogService.logInformation($"로그삭제[LogDelete] {logName}");
            }
            catch (Exception ex)
            {
                LogService.logInformation($"로그삭제실패[LogDelete] {logName} :{ex.Message}");
            }
        }

        static string GetProjectName()
        {
            // 현재 어셈블리를 가져옵니다.
            var assembly = Assembly.GetExecutingAssembly();

            // 어셈블리의 이름을 가져옵니다.
            string assemblyName = assembly.GetName().Name;

            return assemblyName;
        }
    }
}