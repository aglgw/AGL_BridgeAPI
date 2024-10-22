using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models;
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

namespace AGL.Api.API_Template.Services
{
    public class SampleService : BaseService, ISampleService
    {

        private readonly CmsDbContext _context;
        private readonly HttDbContext _httDbContext;
        private readonly AdminContext _adminContext;

        public SampleService(CmsDbContext context
            , HttDbContext httDbContext
            , AdminContext adminContext)
        {
            _context = context;
            _httDbContext = httDbContext;
            _adminContext = adminContext;
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

        public async Task<IDataResult> GetMaria()
        {

            var list = _adminContext.test_Master.ToList();


            return Successed(list);
        }

        public async Task<IDataResult> SetMariaInsertTest(CancellationToken cancellationToken)
        {


            try
            {
                #region insert
                //--------------- 단일 row ---------------
                var obj = new test_Master();

                obj.name = "test";
                _adminContext.test_Master.Add(obj);
                //---------------------------------------


                //--------------- Multi row ---------------
                var objList = new List<test_Master>();

                objList.Add(new test_Master() { name = "test1" });
                objList.Add(new test_Master() { name = "test2" });

                _adminContext.test_Master.AddRange(objList);
                //---------------------------------------



                //--------------- 연관관계  ---------------
                
                



                var obj_2 = new test_Master();


                obj_2.name = "연관";
                obj_2.Detail = new List<test_Detail>()
                            {
                                new test_Detail(){ name="1번", reg_dt=DateTime.Now },
                                new test_Detail(){ name="2번", reg_dt=DateTime.Now, mod_dt=DateTime.Now }
                            };
                _adminContext.test_Master.Add(obj_2);
                //---------------------------------------

                //transation commit 과 동일한 문법
                //에러시 cancellationToken을 통해 일괄 롤백 되어짐. 
                await _adminContext.SaveChangesAsync(cancellationToken);


                #endregion

            }
            catch (Exception ex)
            {
                throw new DomainException(ApplicationCore.Models.Enum.ResultCode.시스템오류, ex.Message);
            }


            return Successed();


        }


        public async Task<IDataResult> SetMariaUpdateTest(CancellationToken cancellationToken)
        {
            try
            {
                #region update
                //--------------- 단일 row ---------------

                var persist = await _adminContext.test_Master.FindAsync(1);

                if (persist != null)
                {
                    persist.name = "변경됨";

                    _adminContext.test_Master.Update(persist);
                }
                //---------------------------------------


                //--------------- Multi row ---------------
                var persistList = await _adminContext.test_Master.Where(x => x.idx > 1 && x.idx < 4).ToListAsync();

                persistList.ForEach(x =>
                {
                    x.name = "변경";
                });

                _adminContext.test_Master.UpdateRange(persistList);
                //---------------------------------------



                //--------------- 연관관계  ---------------
                //연관된 하위 엔티티 update 시 하위 엔티티만 호출해서 update를 한다
                var persistList_2 = await _adminContext.test_Detail.Where(x => x.master_idx == 3).ToListAsync();

                if (persistList_2.Count > 0)
                {
                    persistList_2.ForEach(x =>
                    {
                        x.name = "변경변경";
                        x.mod_dt = DateTime.Now;
                    });

                    _adminContext.test_Detail.UpdateRange(persistList_2);

                }

                //---------------------------------------

                //transation commit 과 동일한 문법
                //에러시 cancellationToken을 통해 일괄 롤백 되어짐. 
                await _adminContext.SaveChangesAsync(cancellationToken);


                #endregion

            }
            catch (Exception ex)
            {
                throw new DomainException(ApplicationCore.Models.Enum.ResultCode.시스템오류, ex.Message);
            }


            return Successed();


        }


        public async Task<IDataResult> DelMariaDeleteTest(CancellationToken cancellationToken)
        {
            try
            {
                #region update
                //--------------- 단일 row ---------------

                var persist = await _adminContext.test_Master.FindAsync(393);

                if (persist != null)
                {
                    _adminContext.test_Master.Remove(persist);
                }
                //---------------------------------------


                //--------------- Multi row ---------------
                var persistList = await _adminContext.test_Master.Where(x => new int[] { 2, 4 }.Contains(x.idx)).ToListAsync();

                _adminContext.test_Master.RemoveRange(persistList);
                //---------------------------------------



                //--------------- 연관관계  ---------------
                //연관된 하위 엔티티 delete 시 하위 엔티티만 호출해서 delete를 한다
                var persistList_2 = await _adminContext.test_Detail.Where(x => x.master_idx == 3).ToListAsync();

                _adminContext.test_Detail.RemoveRange(persistList_2);

                //---------------------------------------

                //transation commit 과 동일한 문법
                //에러시 cancellationToken을 통해 일괄 롤백 되어짐. 
                await _adminContext.SaveChangesAsync(cancellationToken);


                #endregion

            }
            catch (Exception ex)
            {
                throw new DomainException(ApplicationCore.Models.Enum.ResultCode.시스템오류, ex.Message);
            }


            return Successed();


        }


        public async Task<IDataResult> GetMariaMultiSelectSample()
        {
            var list = _adminContext.test_Master
                .Select(x => new testModel   //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                {
                    idx = x.idx,
                    name = x.name,
                    Dtl = x.Detail.Select(x => new testModelDtl
                    {
                        dtlIdx = x.idx,
                        name = x.name,
                        regDt = x.reg_dt,
                        modDt = x.mod_dt
                    }).ToList()
                })
                .ToList();




            return Successed(list);
        }



        public async Task<IDataResult> GetMariaSubEntitySelectSample()
        {
            var list = _adminContext.test_Detail
                .Select(x => new  //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                {
                    idx = x.idx,
                    name = x.name,
                    regDt = x.reg_dt,
                    modDt = x.mod_dt,
                    masterName = x.Master.name
                })
                .ToList();
            return Successed(list);
        }



        public async Task<IDataResult> GetMariaJoinSelectSample()
        {
            //inner join 
            var list_1 = await (from t1 in _adminContext.test_Master
                              join t2 in _adminContext.test_Detail on t1.idx equals t2.master_idx
                              //멀티 컬럼 조인일 경우 아래와 같이 사용 
                              //join t2 in _adminContext.test_Detail on new { idx = t1.idx, name=t1.name } equals new { idx = t2.master_idx, name=t2.name }
                              select new testModel_2 //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                              {
                                  Idx = t1.idx,
                                  Name = t1.name,
                                  DtlIdx = t2.idx,
                                  DtlName = t2.name,
                                  DtlRegDt = t2.reg_dt,
                                  DtlModDt =t2.mod_dt,
                              }).Take(10).ToListAsync();


            // left join 
            var list_2 = await (from t1 in _adminContext.test_Master
                                join t2 in _adminContext.test_Detail on t1.idx equals t2.master_idx into _t2
                                from t2 in _t2.DefaultIfEmpty()
                                    //멀티 컬럼 조인일 경우 아래와 같이 사용 
                                    //join t2 in _adminContext.test_Detail on new { idx = t1.idx, name=t1.name } equals new { idx = t2.master_idx, name=t2.name }
                                select new testModel_2
                                {
                                    Idx = t1.idx,
                                    Name = t1.name,
                                    DtlIdx = t2 != null ? t2.idx : null,
                                    DtlName = t2 != null ? t2.name : null,
                                    DtlRegDt = t2 != null ? t2.reg_dt : null,
                                    DtlModDt = t2 != null ? t2.mod_dt : null,
                                }).Take(10).ToListAsync();

            var list = new
            {
                list_1,
                list_2
            };
    


            return Successed(list);
        }

        

        public async Task<IDataResult> GetMariaSelectExpressionSample()
        {

            //검색시 사용하면 용이함
            //ex> 티타임 조회시 조회조건이 여러가지 일경우
            Expression<Func<test_Master, bool>> pred = x => true;
            Expression<Func<test_Master, bool>> pred_2 = x => true;

            //조회조건의 파라미터에 if 문 부분
            {
                pred = x => x.name.Contains("연관");
                //
            }

            
            {
                pred_2 = x => x.idx==1;
                //
            }

            var list = await _adminContext.test_Master
                .Where(pred)
                .Where(pred_2)
                .Select(x => new  //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                {
                    idx = x.idx,
                    name = x.name,
                    Dtl = x.Detail.Select(x => new
                    {
                        dtlIdx = x.idx,
                        name = x.name,
                        regDt = x.reg_dt,
                        modDt = x.mod_dt
                    })
                }).ToListAsync();

            return Successed(list);
        }



        public async Task<IDataResult> GetMariaSelectQueryFilterSample()
        {

            //검색시 사용하면 용이함
            //ex> 티타임 조회시 조회조건이 여러가지 일경우
            Expression<Func<test_Master, bool>> pred = x => true;

            //조회조건의 파라미터에 if 문 부분
            {
                pred = x => x.name.Contains("변경");
                //
            }

            var searchQry = new DomainQuery();
            searchQry.QueryFilters.Add(new QueryFilter { Field = "name", Value = "test" });
            searchQry.QueryFilters.Add(new QueryFilter { Field = "idx", Value = "3" });

            var list = await _adminContext.test_Master
                .WhereInFilter(searchQry.QueryFilters)
                .Select(x => new  //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                {
                    idx = x.idx,
                    name = x.name,
                    Dtl = x.Detail.Select(x => new
                    {
                        dtlIdx = x.idx,
                        name = x.name,
                        regDt = x.reg_dt,
                        modDt = x.mod_dt
                    })
                }).ToListAsync();

            return Successed(list);
        }


        public async Task<IDataResult> GetMariaSelectPagingSample(DomainQuery query)
        {
            var list = await _adminContext.test_Master
                //.WhereOrFilterT(query.QueryFilters)  // or 검색
                .WhereInFilter(query.QueryFilters) // and 검색
                .Select(x => new  //dynamic 말고 되도록이면 모델링을 통해서 하기~~~  new 모델레코드
                {
                    idx = x.idx,
                    name = x.name,
                    Dtl = x.Detail.Select(x => new
                    {
                        dtlIdx = x.idx,
                        name = x.name,
                        regDt = x.reg_dt,
                        modDt = x.mod_dt
                    })
                })
                .OrderByDynamic(query.SortKey, query.SortDirection)
                .PaginatedListAsync(query.PageIndex * query.PageSize, query.PageSize);

            return Successed(list);

        }

        public async Task<IDataResult> SetMariaBulkSample()
        {

            var insList = new List<test_Master>();


            for(var i=0; i<=1000; i++)
            {

                insList.Add(new test_Master
                {
                    name = $"이름_{i}",
                    Detail = new List<test_Detail>() { new test_Detail { name=$"상세 이름_1_{i}", reg_dt=DateTime.Now }, new test_Detail { name = $"상세 이름_2_{i}", reg_dt = DateTime.Now } }
                });

            }

            if (insList.Count > 0)
            {

                //---------------------- 단일 테이블 Bulk Insert ------------------------
                await _adminContext.BulkInsertAsync(insList);
                //-----------------------------------------------------------------------





                //---------------------- 연관 관계 Bulk Insert ------------------------
                var strategy = _adminContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _adminContext.Database.BeginTransactionAsync())
                    {
                        _adminContext.BulkInsert(insList, option => { option.IncludeGraph = true; });
                    }
                });
                //----------------------------------------------------------------------

            }



            return Successed();
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


    }
        
}
