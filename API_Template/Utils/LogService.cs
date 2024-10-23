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
        /// 유틸로그
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="logTitle"></param>
        /// <param name="logTxt"></param>
        /// <param name="isError"></param>
        /// <param name="file"></param>
        /// <param name="line"></param>
        /// <param name="member"></param>
        public static void LogReg(string logName, string logTitle, string logTxt, bool isError = false, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            try
            {
                logTitle = $"File: {file} (Line: {line}) {member} : {logTitle}";
                Logs.LogReg(GetProjectName(), logName, logTitle, logTxt, isError);
            }
            catch (Exception ex)
            {
                LogService.logInformation($"로그실패[LogReg]({logName}):{ex.Message}");
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