using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Azure;
using Microsoft.EntityFrameworkCore;
using RTools_NTS.Util;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Services
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        public const string AuthToken = "0ksP6iZltO"; // 인증용 header token
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        //private readonly RequestQueue _queue;

        public AuthenticationService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
        }

        public async Task<IDataResult> PostAuthentication(AuthenticationRequest request, string token)
        {
            if (string.IsNullOrEmpty(token) || token != AuthToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        /*
                        1 인증 코드 유효성 검사
                        2 타입값에 따른 코드 분류
                        3 토큰 생성
                         */
                        var authType = request.authType;
                        object auth = null;
                        if (authType == "1") // 공급사
                        {
                            auth = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == request.authCode);
                        }
                        else if (authType == "2") // 클라이언트
                        {
                            auth = await _context.SyncClients.FirstOrDefaultAsync(s => s.ClientName == request.authCode);
                        }

                        if (auth != null)
                        {
                            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "authCode is duplicated ", null);
                        }
                        else
                        {
                            if (authType == "1") // 공급사
                            {
                                var newSupplier = new OAPI_Supplier
                                {
                                    SupplierCode = request.authCode,
                                    EndPoint = request.endPoint,
                                    CreatedDate = DateTime.UtcNow,
                                };
                                _context.Suppliers.Add(newSupplier);
                                await _context.SaveChangesAsync();
                                int SupplierId = newSupplier.SupplierId;

                                var newAuthentication = new OAPI_Authentication
                                {
                                    SupplierId = SupplierId,
                                    TokenSupplier = GenerateRandomNumber(),
                                    //TokenClient = "",
                                    AglCode = "AGL0001",
                                    TokenAgl = GenerateRandomNumber(),
                                    Deleted = false,
                                    CreatedDate = DateTime.UtcNow,
                                };
                                _context.Authentications.Add(newAuthentication);
                                await _context.SaveChangesAsync();
                            }
                            else if (authType == "2") // 클라이언트
                            {
                                auth = await _context.SyncClients.FirstOrDefaultAsync(s => s.ClientName == request.authCode);

                                var maxSyncTeeTimeMappingId = await _context.SyncTeeTimeMappings.MaxAsync(s => s.TeetimeMappingId);

                                var newSyncClient = new OAPI_SyncClient
                                {
                                    ClientName = request.authCode,
                                    ClientEndpoint = request.endPoint,
                                    LastSyncTeeTimeMappingId = maxSyncTeeTimeMappingId
                                };

                                _context.SyncClients.Add(newSyncClient);
                                await _context.SaveChangesAsync();
                                int SyncClientId = newSyncClient.SyncClientId;

                                var newAuthentication = new OAPI_Authentication
                                {
                                    SyncClientId = SyncClientId,
                                    //TokenSupplier = GenerateRandomNumber(),
                                    TokenClient = GenerateRandomNumber(),
                                    //AglCode = "AGL0001",
                                    //TokenAgl = GenerateRandomNumber(),
                                    Deleted = false,
                                    CreatedDate = DateTime.UtcNow,
                                };
                                _context.Authentications.Add(newAuthentication);
                                await _context.SaveChangesAsync();
                            }
                        }

                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(request.authCode, request.authCode, $"Auth", $"인증 생성 성공");
                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "PostAuthentication successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(request.authCode, request.authCode, $"Auth", $"인증 생성 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });

        }


        public async Task<IDataResult> GetAuthentication(AuthenticationRequest request,string token)
        {
            if (string.IsNullOrEmpty(token) || token != AuthToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            try
            {
                var authType = request.authType;
                
                if(authType == "1") // 공급사
                {
                    var supplier = await _context.Suppliers.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.SupplierCode == request.authCode);

                    var response = new authAuthenticationResponse
                    {
                        authCode = supplier.SupplierCode,
                        TokenSupplier = supplier.Authentication.TokenSupplier,
                        TokenClient = supplier.Authentication.TokenClient,
                        AglCode = supplier.Authentication.AglCode,
                        TokenAgl = supplier.Authentication.TokenAgl,
                    };

                    return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "Authentication List successfully", response);

                }
                else if(authType == "2") // 클라이언트
                {
                    var syncClient = await _context.SyncClients.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.ClientName == request.authCode);

                    var response = new authAuthenticationResponse
                    {
                        authCode = syncClient.ClientName,
                        TokenSupplier = syncClient.Authentication.TokenSupplier,
                        TokenClient = syncClient.Authentication.TokenClient,
                        AglCode = syncClient.Authentication.AglCode,
                        TokenAgl = syncClient.Authentication.TokenAgl,
                    };

                    return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "Authentication List successfully", response);
                }


                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Authentication List successfully", null);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(request.authCode, request.authCode, $"Authentication", $"인증 리스트 생성 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }


        private static string GenerateRandomNumber(int length = 20)
        {
            string guid = Guid.NewGuid().ToString("N"); // 32자리 고유 문자열
            return guid.Substring(0, length);
            //Random random = new Random();
            //string result = string.Concat(Enumerable.Range(0, length)
            //    .Select(_ => random.Next(0, 10).ToString()));
            //return result;
        }
    }
}
