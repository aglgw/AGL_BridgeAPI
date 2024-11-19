using AGL.Api.ApplicationCore.Utilities;
//using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace AGL.Api.ApplicationCore.Helpers
{
    /// <summary>
    /// 통신
    /// </summary>
    public class RestfulClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static async Task POST<T>(string url, object obj, Action<HttpStatusCode, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            T retValue = new T();
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonSerializer.Serialize(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        string responseData = await response.Content.ReadAsStringAsync();
                        retValue = JsonSerializer.Deserialize<T>(responseData);
                    }
                    else
                    {

                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.ToString());
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POST WebException ex : ", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POST WebException ex.Response : ", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > POST WebException ex : {ex.Message}");
            }
            catch (Exception ex)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POST Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POST Exception ex : {ex.Message}");
            }

            action(httpStatusCode, retValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="values"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task POSTAPIForm<T>(string url, Dictionary<string, string> values, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();

            try
            {
                using (var client = new HttpClient())
                {
                    // 타임아웃 30초
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // form으로 변환
                    var content = new FormUrlEncodedContent(values);

                    //HTTP POST
                    var result = await client.PostAsync(url, content);

                    //Logs.LogRegDay("RestfulClient", "RestfulClient > POSTFormAPI > result : ", result.ToString());

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        //결과값확인
                        //Logs.LogRegDay("RestfulClient", $"RestfulClient > POSTFormAPI > result readTask", readTask);
                        readTask = readTask.Replace(",]", "]");
                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(readTask);
                    }
                    else {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = result.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.Message);
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                reasonPhrase = ex.Message;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTFormAPI WebException ex : ", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTFormAPI WebException ex.Response : ", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > POSTAPIForm WebException ex : {ex.Message}");
            }
            catch (Exception ex)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                reasonPhrase = ex.Message;
                //Debug.WriteLine("EX:" + ex.Message);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTFormAPI Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POSTAPIForm Exception ex : {ex.Message}");
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// 토큰인증 포함 POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="oauthToken"></param>
        /// <param name="content"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task POSTAPIToken<T>(string url, string oauthToken, StringContent content, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();

            try
            {
                using (var client = new HttpClient())
                {
                    // 타임아웃 30초
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oauthToken);

                    //HTTP POST
                    var result = await client.PostAsync(url, content);

                    //Logs.LogRegDay("RestfulClient", "RestfulClient > POSTAPIToken > result : ", result.ToString());

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        //결과값확인
                        //Logs.LogRegDay("RestfulClient", $"RestfulClient > POSTAPIToken > result readTask", readTask);

                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(readTask);
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = result.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIToken WebException ex :", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIToken WebException ex.Response :", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > POSTAPIToken WebException ex : {ex.Message}");
                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.Message);
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIToken Exception ex :", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POSTAPIToken Exception ex : {ex.Message}");
                httpStatusCode = HttpStatusCode.InternalServerError;
                reasonPhrase = ex.Message;
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Header[key,value] POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="dictinary"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task POSTAPIHeader(string url, Dictionary<string, string> dictinary, Action<HttpStatusCode, string, string> action)
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = "";
            string reasonValue = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    foreach (var kv in dictinary)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    //LogService.logInformation($"try [RestfulClient > POSTAPI url]({url})");
                    var response = await client.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        //var data = response.Content.ReadAsStringAsync().Result;
                        string data = await response.Content.ReadAsStringAsync();
                        //Data확인
                        //LogService.logInformation($"RestfulClient RestfulClient > POSTAPI data {data}");
                        //LogService.logInformation($"success [RestfulClient > POSTAPI]({url})");
                        reasonPhrase = "";
                        reasonValue = data;
                    }
                    else
                    {
                        LogService.logInformation($"fail [RestfulClient > POSTAPI]({response.ReasonPhrase})");
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = response.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                if (response == null)
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    httpStatusCode = response.StatusCode;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPI WebException ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POSTAPI WebException ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETAPI Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPI Exception ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            action(httpStatusCode, reasonPhrase, reasonValue);
        }

        /// <summary>
        /// Header[key,value], Form[key,value] POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task POSTAPIHeaderForm<T>(string url, Dictionary<string, string> header, Dictionary<string, string> query, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();

            try
            {
                using (var client = new HttpClient())
                {
                    // 타임아웃 30초
                    client.Timeout = TimeSpan.FromSeconds(30);

                    //header
                    foreach (var kv in header)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    // form
                    var content = new FormUrlEncodedContent(query); 

                    //HTTP POST
                    var result = await client.PostAsync(url, content);

                    //Logs.LogRegDay("RestfulClient", "RestfulClient > POSTFormAPI > result : ", result.ToString());

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        //결과값확인
                        //Logs.LogRegDay("RestfulClient", $"RestfulClient > POSTFormAPI > result readTask", readTask);
                        readTask = readTask.Replace(",]", "]");
                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(readTask);
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = result.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.Message);
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                reasonPhrase = ex.Message;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm WebException ex : ", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm WebException ex.Response : ", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > POSTAPIHeaderForm WebException ex : {ex.Message}");
            }
            catch (Exception ex)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                reasonPhrase = ex.Message;
                //Debug.WriteLine("EX:" + ex.Message);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POSTAPIHeaderForm Exception ex : {ex.Message}");
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Header[key,value], Form[key,value] PUT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task PUTAPIHeaderForm<T>(string url, Dictionary<string, string> header, Dictionary<string, string> query, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();

            try
            {
                using (var client = new HttpClient())
                {
                    // 타임아웃 30초
                    client.Timeout = TimeSpan.FromSeconds(30);

                    //header
                    foreach (var kv in header)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    // form
                    var content = new FormUrlEncodedContent(query);

                    //HTTP PUT
                    var result = await client.PutAsync(url, content);


                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        readTask = readTask.Replace(",]", "]");
                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(readTask);
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = result.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.Message);
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                reasonPhrase = ex.Message;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > PUTAPIHeaderForm WebException ex : ", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > PUTAPIHeaderForm WebException ex.Response : ", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > PUTAPIHeaderForm WebException ex : {ex.Message}");
            }
            catch (Exception ex)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                reasonPhrase = ex.Message;
                //Debug.WriteLine("EX:" + ex.Message);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > PUTAPIHeaderForm Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > PUTAPIHeaderForm Exception ex : {ex.Message}");
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Header[key,value], Form[key,value] POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task POSTAPIHeaderObject<T>(string url, Dictionary<string, string> header, object obj, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();

            try
            {
                using (var client = new HttpClient())
                {
                    // 타임아웃 30초
                    client.Timeout = TimeSpan.FromSeconds(30);

                    //header
                    foreach (var kv in header)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    // form
                    string json = JsonSerializer.Serialize(obj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    //HTTP POST
                    var result = await client.PostAsync(url, content);

                    //Logs.LogRegDay("RestfulClient", "RestfulClient > POSTFormAPI > result : ", result.ToString());

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync().Result;
                        //결과값확인
                        //Logs.LogRegDay("RestfulClient", $"RestfulClient > POSTFormAPI > result readTask", readTask);
                        readTask = readTask.Replace(",]", "]");
                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(readTask);
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = result.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;

                if (response != null)
                {
                    httpStatusCode = response.StatusCode;
                }
                else
                {
                    Debug.WriteLine("EX:" + ex.Message);
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }

                reasonPhrase = ex.Message;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm WebException ex : ", ex.Message, true);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm WebException ex.Response : ", ex.Response.ToString(), true);
                LogService.logInformation($"fail RestfulClient > POSTAPIHeaderForm WebException ex : {ex.Message}");
            }
            catch (Exception ex)
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
                reasonPhrase = ex.Message;
                //Debug.WriteLine("EX:" + ex.Message);
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > POSTAPIHeaderForm Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > POSTAPIHeaderForm Exception ex : {ex.Message}");
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Header[key,value] GET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="dictinary"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task GETAPI(string url, Dictionary<string, string> dictinary, Action<HttpStatusCode, string, string> action)
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = "";
            string reasonValue = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    foreach (var kv in dictinary)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    //LogService.logInformation($"try [Restfull > GETAPI url]({url})");
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        //var data = response.Content.ReadAsStringAsync().Result;
                        string data = await response.Content.ReadAsStringAsync();
                        //Data확인
                        //Logs.LogRegDay("RestfulClient", $"Restfull > GETtoken data", data);
                        //LogService.logInformation($"success [Restfull > GETAPI]({url})");
                        reasonPhrase = "";
                        reasonValue = data;
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = response.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                if (response == null)
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    httpStatusCode = response.StatusCode;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETAPI WebException ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPI WebException ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETAPI Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPI Exception ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            action(httpStatusCode, reasonPhrase, reasonValue);
        }

        /// <summary>
        /// 토큰인증 포함 GET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="oauthToken"></param>
        /// <param name="parameter"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task GETAPIToken<T>(string url, string oauthToken, string parameter, Action<HttpStatusCode,string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oauthToken);

                    url += parameter;
                    //Log.LogRegDay("RestfulClient", $"Restfull>GETtoken url", url);
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsStringAsync().Result;

                        //Data확인
                        //Log.LogRegDay("RestfulClient", $"Restfull>GETtoken data", data);
                        retValue = System.Text.Json.JsonSerializer.Deserialize<T>(data);
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = response.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                if (response == null)
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    httpStatusCode = response.StatusCode;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETAPIToken WebException ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPIToken WebException ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETtoken Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPIToken Exception ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Header[key,value], parameter GET
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="query"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task GETAPIHeaderQuery<T>(string url, Dictionary<string, string> header, Dictionary<string, string> query, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T retValue = new T();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    foreach (var kv in header)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }

                    var keyValuePairs = new List<string>();
                    foreach (var parameter in query)
                    {
                        keyValuePairs.Add($"{parameter.Key}={parameter.Value}");
                    }

                    var parameters = string.Join("&", keyValuePairs);

                    url += "?" + parameters;

                    //Log.LogRegDay("RestfulClient", $"Restfull>GETtoken url", url);
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsStringAsync().Result;

                        //Data확인
                        //Log.LogRegDay("RestfulClient", $"Restfull>GETtoken data", data);
                        //!@#$시간날때 공통으로 고쳐놓자..!@#$
                        if (url.Contains("https://oapi.hdc-resort.com/golfota") && data.Contains("\"userStatus\": {\"code\": 2000,\"message\": \"OK\"}"))
                        {
                            retValue = System.Text.Json.JsonSerializer.Deserialize<T>(data);
                        }
                        else
                        {
                            data = data.Replace("\"대상이되는 예약 내역이 없습니다.\"", "[]");
                            retValue = System.Text.Json.JsonSerializer.Deserialize<T>(data);
                        }
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = response.ReasonPhrase;
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                if (response == null)
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    httpStatusCode = response.StatusCode;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETAPIToken WebException ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPIToken WebException ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > GETtoken Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > GETAPIToken Exception ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            action(httpStatusCode, reasonPhrase, retValue);
        }

        /// <summary>
        /// Soap
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="strRequest"></param>
        /// <param name="strExec"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task SoapEnvelope<T>(string url, string strRequest, string strExec, Action<HttpStatusCode, string, T> action) where T : new()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            string reasonPhrase = string.Empty;
            T resultValue = new T();

            string xmlResult = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpContent content = new StringContent(strRequest, Encoding.UTF8, "application/soap+xml");
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        httpStatusCode = HttpStatusCode.OK;
                        xmlResult = await response.Content.ReadAsStringAsync();
                        if (strExec == "GetClubName") { 
                        //"numPage":, 없는건 왜 넣은건지...
                        xmlResult = xmlResult.Replace("\"numPage\":,", "");
                        }
                        //UtilLogs.LogRegDay($"{headerDaemonId}", $"", $"soapEnvelope 성공");
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.InternalServerError;
                        reasonPhrase = response.ReasonPhrase;
                    }
                }

                if (httpStatusCode == HttpStatusCode.OK) { 
                    XDocument doc = XDocument.Parse(xmlResult);

                    // XML 네임스페이스를 정의
                    XNamespace ns = "https://vin.golfsoft.vn/";

                    // GetClubDetailResult 요소를 찾고 그 안의 텍스트를 추출
                    var resultElement = doc.Descendants(ns + $"{strExec}Result").FirstOrDefault();
                    if (resultElement != null)
                    {
                        string resultJson = resultElement.Value;

                        //// JSON 문자열에서 description 값을 추출
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(resultJson);
                        //string description = jsonDoc.RootElement.GetProperty("description").GetString();
                        //if (description == "Success")
                        //{
                        //    resultValue = jsonDoc.RootElement.GetProperty("result").ToString();
                        //    //string jsonString = System.Text.Json.JsonSerializer.Serialize(resultValue);
                        //    //UtilLogs.LogRegDay($"{headerDaemonId}", $"", $"골프장상세조회 성공 : {strResponse}");

                        //    resultValue = System.Text.Json.JsonSerializer.Deserialize<T>(resultValue);
                        //}
                        //else
                        //{
                        //    httpStatusCode = HttpStatusCode.BadRequest;
                        //    reasonPhrase = xmlResult;
                        //}
                        resultValue = System.Text.Json.JsonSerializer.Deserialize<T>(jsonDoc.RootElement.ToString());
                    }
                    else
                    {
                        httpStatusCode = HttpStatusCode.BadRequest;
                        reasonPhrase = $"{strExec}Result Not Found";
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                if (response == null)
                {
                    httpStatusCode = HttpStatusCode.BadRequest;
                }
                else
                {
                    httpStatusCode = response.StatusCode;
                }

                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > SoapEnvelope WebException ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > SoapEnvelope WebException ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EX:" + ex.Message);
                httpStatusCode = HttpStatusCode.InternalServerError;
                Logs.LogRegDay("ApplicationCore", "RestfulClient", "RestfulClient", "RestfulClient > SoapEnvelope Exception ex : ", ex.Message, true);
                LogService.logInformation($"fail RestfulClient > SoapEnvelope Exception ex : {ex.Message}");
                reasonPhrase = ex.Message;
            }

            action(httpStatusCode, reasonPhrase, resultValue);
        }
    }

    public class APICall
    {
        public static MemoryStream ObjectToJson(object obj)
        {
            return objectToJson(obj);
        }

        public static object JsonToObject(string jsonString, object obj)
        {
            return jsonToObject(jsonString, obj);
        }

        public static string ObjectToJsonString(object obj)
        {
            return objectToJsonString(obj);
        }

        private static string objectToJsonString(object obj)
        {
            MemoryStream strm = objectToJson(obj);
            int len = (int)strm.Length;
            byte[] bt = new byte[len];
            strm.Position = 0;
            strm.Read(bt, 0, len);
            return Encoding.UTF8.GetString(bt);
        }

        private static MemoryStream objectToJson(object obj)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(stream, obj);
                return stream;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static object jsonToObject(string jsonString, object obj)
        {
            Debug.WriteLine(obj.GetType().ToString());
            try
            {
                MemoryStream strm = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
                object retobj = ser.ReadObject(strm);
                strm.Close();
                return retobj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
