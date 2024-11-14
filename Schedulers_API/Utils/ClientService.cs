using AGL.Api.ApplicationCore.Helpers;
using AGL.Api.ApplicationCore.Utilities;

namespace AGL.Api.Schedulers_API.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class Client
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="controller"></param>
        /// <param name="service"></param>
        /// <param name="logPath"></param>
        /// <returns></returns>
        public static async Task<string> GETAPI(string serviceCode, string controller, string service, string logPath)
        {
            string jsonString = string.Empty;
            string urlBase = string.Empty;
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            switch (environment)
            {
                case "Production":
                    urlBase = "https://field-inbound-api.tigergds.com";
                    break;
                case "Staging":
                    urlBase = "https://dev-field-inbound-api.tigergds.com";
                    break;
                default:
                    urlBase = "https://localhost:7011";
                    break;
            }

            urlBase += $"/{controller}/{service}";

            try
            {
                // Dictionary<string, string> 초기화
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    { "Id", serviceCode }
                };

                UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"try ({urlBase})");
                //LogService.logInformation($"try [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                await RestfulClient.GETAPI(urlBase, dictionary, (status, reasonPhrase, apiResponse) =>
                {
                    if (status == System.Net.HttpStatusCode.OK)
                    {
                        jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                        //var response = System.Text.Json.JsonSerializer.Deserialize<OtaAPIResponse>(apiResponse);
                        //if (response != null && response.resultCode == 0)
                        //{
                        //    UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"success ({urlBase}) apiResponse : {apiResponse}");
                        //    //LogService.logInformation($"success [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                        //}
                        //else
                        //{
                        //    UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {apiResponse}", true);
                        //    LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase})");
                        //}
                        jsonString = apiResponse;
                    }
                    else
                    {
                        UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {reasonPhrase}", true);
                        LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase}):{reasonPhrase}");
                    }
                });
            }
            catch (Exception ex)
            {
                UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) Exception : {ex.Message}", true);
                LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase}):{ex.Message}");
            }

            return jsonString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="controller"></param>
        /// <param name="service"></param>
        /// <param name="logPath"></param>
        /// <returns></returns>
        public static async Task<string> GETOutboundAPI(string serviceCode, string controller, string service, string logPath)
        {
            string jsonString = string.Empty;
            string urlBase = string.Empty;
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            switch (environment)
            {
                case "Production":
                    urlBase = "https://field-outbound-api.tigergds.com";
                    break;
                case "Staging":
                    urlBase = "https://dev-field-outbound-api.tigergds.com";
                    break;
                default:
                    urlBase = "https://localhost:7012";
                    break;
            }

            urlBase += $"/{controller}/{service}";

            try
            {
                // Dictionary<string, string> 초기화
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    { "Id", serviceCode }
                };

                UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"try ({urlBase})");
                //LogService.logInformation($"try [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                await RestfulClient.GETAPI(urlBase, dictionary, (status, reasonPhrase, apiResponse) =>
                {
                    if (status == System.Net.HttpStatusCode.OK)
                    {
                        jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                        //var response = System.Text.Json.JsonSerializer.Deserialize<OtaAPIResponse>(apiResponse);
                        //if (response != null && response.resultCode == 0)
                        //{
                        //    UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"success ({urlBase}) apiResponse : {apiResponse}");
                        //    //LogService.logInformation($"success [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                        //}
                        //else
                        //{
                        //    UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {apiResponse}", true);
                        //    LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase})");
                        //}
                        jsonString = apiResponse;
                    }
                    else
                    {
                        UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {reasonPhrase}", true);
                        LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase}):{reasonPhrase}");
                    }
                });
            }
            catch (Exception ex)
            {
                UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) Exception : {ex.Message}", true);
                LogService.logInformation($"fail [ {serviceCode} > GetTeeTimeList ]({urlBase}):{ex.Message}");
            }

            return jsonString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="controller"></param>
        /// <param name="service"></param>
        /// <param name="logPath"></param>
        /// <returns></returns>
        public static async Task<string> POSTOutboundAPI(string serviceCode, string controller, string service, string logPath)
        {
            string jsonString = string.Empty;
            string urlBase = string.Empty;
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            switch (environment)
            {
                case "Production":
                    urlBase = "https://field-outbound-api.tigergds.com";
                    break;
                case "Staging":
                    urlBase = "https://dev-field-outbound-api.tigergds.com";
                    break;
                default:
                    urlBase = "https://localhost:7012";
                    break;
            }

            urlBase += $"/{controller}/{service}";

            try
            {
                // Dictionary<string, string> 초기화
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    { "Id", serviceCode }
                };

                UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"try ({urlBase})");
                //LogService.logInformation($"try [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                await RestfulClient.POSTAPIHeader(urlBase, dictionary, (status, reasonPhrase, apiResponse) =>
                {
                    if (status == System.Net.HttpStatusCode.OK)
                    {
                        var jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                        //var response = System.Text.Json.JsonSerializer.Deserialize<OtaAPIResponse>(apiResponse);
                        //if (response != null && response.resultCode == 0)
                        //{
                        //    UtilLogs.LogRegDay("ClientService", $"{serviceCode}{logPath}", $"", $"success ({urlBase}) apiResponse : {apiResponse}");
                        //    //LogService.logInformation($"success [ Phoenixhnr > GetTeeTimeList ]({urlBase})");
                        //}
                        //else
                        //{
                        //    UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {apiResponse}", true);
                        //    LogService.logInformation($"fail [ {serviceCode} > POSTOutboundAPI ]({urlBase})");
                        //}
                        //jsonString = apiResponse;
                    }
                    else
                    {
                        UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) apiResponse : {reasonPhrase}", true);
                        LogService.logInformation($"fail [ {serviceCode} > POSTOutboundAPI ]({urlBase}):{reasonPhrase}");
                    }
                });
            }
            catch (Exception ex)
            {
                UtilLogs.LogRegDay("ClientService", serviceCode, $"", $"fail ({urlBase}) Exception : {ex.Message}", true);
                LogService.logInformation($"fail [ {serviceCode} > POSTOutboundAPI ]({urlBase}):{ex.Message}");
            }

            return jsonString;
        }
    }
}