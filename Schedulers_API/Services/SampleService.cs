using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Utilities;
using AGL.Api.Schedulers_API.Interfaces;
using AGL.Api.Schedulers_API.Models;
using AGL.Api.Infrastructure.Data;
using StoredProcedureEFCore;
using System.Net.NetworkInformation;

namespace AGL.Api.Schedulers_API.Services
{
    public class SampleService : BaseService, ISampleService
    {

        private readonly CmsDbContext _context;
        public SampleService(CmsDbContext context)
        {
            _context = context;
        }

        public async Task<IDataResult> GetCheckInTeeTimeList()
        {
            var list = _context.TA_Checkin_TeeTime.Take(5).ToList();
            var localIP = GetIPAddresses();
            LogService.logInformation("Local IP Address: " + localIP);
            return Successed(localIP);
        }

        public async Task<IDataResult> CallSp(string fieldId)
        {

            var rows = new List<ShopList>();
            try
            {
                _context.LoadStoredProc("dbo.APISP_Shop_GetList")
                     .AddParam("@FieldId", fieldId)
                     .Exec(r => rows = r.ToList<ShopList>());
            }
            catch (Exception ex)
            {
                LogService.logError($"err- {ex.Message}");
            }


            return Successed(rows);
        }

        public async Task Test(string str)
        {
            //LogService.logInformation($"{str}-{DateTime.Now.ToString("yyyyMMddHHmmss")}");
            for (int i = 0; i < 10; i++)
            {
                MethodAsync(i * 1000);
            }

            var localIP = GetIPAddresses();
            LogService.logInformation("Local IP Address: " + localIP);

            LogService.logInformation($"Test Quartz completed.-{DateTime.Now.ToString("yyyyMMddHHmmss")}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string? GetIPAddresses()
        {
            string IpAddresses = string.Empty;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            //공인IP
            IpAddresses = networkInterfaces
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ua => ua.Address.ToString())
                .FirstOrDefault();
            //로컬IP
            //IpAddresses = Dns.GetHostEntry(Dns.GetHostName());
            //return host.AddressList
            //    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    .Select(ip => ip.ToString())
            //    .FirstOrDefault();

            return IpAddresses;

        }

        static async Task MethodAsync(int delay)
        {
            await Task.Delay(delay); // Simulate some work
            LogService.logInformation($"Method {delay} completed.-{DateTime.Now.ToString("yyyyMMddHHmmss")}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IDataResult> TestProtocol()
        {
            string jsonString = string.Empty;
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return Successed(environment);
        }
    }
}
