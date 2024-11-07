using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Queries;
using AGL.Api.ApplicationCore.Utilities;
using AGL.Api.Domain.Entities;
using AGL.Api.HTT.Models;
using AGL.Api.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using StoredProcedureEFCore;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using AGL.Api.Bridge_API.Utils;
using System.Reflection;

namespace AGL.Api.Bridge_API.Services
{
    public class SampleService : BaseService, ISampleService
    {

        private readonly CmsDbContext _context;
        private readonly HttDbContext _httDbContext;

        public SampleService(CmsDbContext context
            , HttDbContext httDbContext)
        {
            _context = context;
            _httDbContext = httDbContext;
        }


        public async Task<IDataResult> GetCheckInTeeTimeList()
        {
            var list = _context.TA_Checkin_TeeTime.Take(5).ToList();


            return Successed(list);
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


        public async Task<IDataResult> GetHTT()
        {
            var list = _httDbContext.HTT_CURRENCY_CODE.Take(5).ToList();

            return Successed(list);
        }

        public async Task MigNation()
        {



            var langList = new List<LanguageMappingByNation>();


            using (var httpClient = new HttpClient())
            {
                var url = "https://restcountries.com/v3.1/all";

                var response = await httpClient.GetAsync(url);

                var responseString = await response.Content.ReadAsStringAsync();

                var jsonResult = JArray.Parse(responseString);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {


                    foreach (JObject item in jsonResult)
                    {

                        try
                        {
                            langList.Add(new LanguageMappingByNation
                            {
                                nationCd = item["cca2"].ToString().ToUpper(),
                                langCd = item["languages"]?.Last.Path.Split('.')[2].ToUpper()
                            });
                        }
                        catch (Exception ex)
                        {

                            LogService.logError(JsonConvert.SerializeObject(item));
                        }


                    }
                }

            }



            var list = _httDbContext.HTT_CODE_NATION.Where(x=>string.IsNullOrEmpty(x.LANG_CD)).ToList();

            list.ForEach(lang =>
            {
                lang.LANG_CD = langList.Where(x => x.nationCd == lang.NAT_CD).Select(x => x.langCd).FirstOrDefault();

            });


            list = list.Where(x => !string.IsNullOrEmpty(x.LANG_CD)).ToList();

            //_httDbContext.HTT_CODE_NATION.Update(obj);
            _httDbContext.HTT_CODE_NATION.UpdateRange(list);
            await _httDbContext.SaveChangesAsync();



        }

        public async Task<IDataResult> GetEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // 현재 어셈블리를 가져옵니다.
            var assembly = Assembly.GetExecutingAssembly();

            // 어셈블리의 이름을 가져옵니다.
            var assemblyName = assembly.GetName().Name;


            UtilLogs.LogRegDay($"Sample", $"GetEnvironment", $"environment : {environment} // assemblyName : {assemblyName}", "");
            return Successed($"environment : {environment} // assemblyName : {assemblyName}");
        }
    }
        
}
