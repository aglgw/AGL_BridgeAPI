using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Services
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        public const string AuthToken = "0ksP6iZltO"; // 인증용 header token
        private readonly string _filePath = @"C:\AGL\DATA\agl_code.txt"; // AGLCODE 생성을 위한 파일 (파일에서 숫자 증가 방식)
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        private readonly PersistentAglCodeGenerator _aglCodeGenerator; // AglCode 생성기


        public AuthenticationService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;

            // PersistentAglCodeGenerator 초기화
            _aglCodeGenerator = new PersistentAglCodeGenerator(_filePath);
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
                        var authType = request.authType;

                        if (authType == "1") // 공급사
                        {
                            var supplierCode = "";
                            if (string.IsNullOrWhiteSpace(request.authCode)) // 코드가 없을시 SUP + 랜덤문자열8 자리로 생성
                            {
                                supplierCode = "SUP" + GenerateRandomNumber(8);
                            }
                            else
                            {
                                supplierCode = request.authCode;
                                // 공급사 코드 중복체크
                                var auth = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == request.authCode);
                                if (auth != null)
                                {
                                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "authCode is duplicated ", null);
                                }
                            }
                            
                            // 공급사 생성
                            var newSupplier = new OAPI_Supplier
                            {
                                SupplierCode = supplierCode,
                                SupplierName = request.authName,
                                EndPoint = request.endPoint,
                                CreatedDate = DateTime.UtcNow,
                            };
                            _context.Suppliers.Add(newSupplier);
                            await _context.SaveChangesAsync();
                            int SupplierId = newSupplier.SupplierId;

                            // 인증 생성
                            var newAuthentication = new OAPI_Authentication
                            {
                                SupplierId = SupplierId,
                                TokenSupplier = GenerateRandomNumber(),
                                //TokenClient = "",
                                AglCode = _aglCodeGenerator.GetNextAglCode(),
                                TokenAgl = GenerateRandomNumber(),
                                Deleted = false,
                                CreatedDate = DateTime.UtcNow,
                            };
                            _context.Authentications.Add(newAuthentication);
                            await _context.SaveChangesAsync();
                        }
                        else if (authType == "2") // 클라이언트
                        {
                            var ClientCode = "";
                            if (string.IsNullOrWhiteSpace(request.authCode))  // 코드가 없을시 CET + 랜덤문자열8 자리로 생성
                            {
                                ClientCode = "CET" + GenerateRandomNumber(8);
                            }
                            else
                            {
                                ClientCode = request.authCode;
                                // 클라이언트 코드 중복체크
                                var auth = await _context.SyncClients.FirstOrDefaultAsync(s => s.ClientCode == request.authCode);
                                if (auth != null)
                                {
                                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "authCode is duplicated ", null);
                                }
                            }

                            // 클라이언트 생성 시 동기티타임의 가장 큰값으로 저장
                            var maxSyncTeeTimeMappingId = await _context.SyncTeeTimeMappings.MaxAsync(s => s.TeetimeMappingId);

                            // 클라이언트 생성
                            var newSyncClient = new OAPI_SyncClient
                            {
                                ClientCode = ClientCode,
                                ClientName = request.authName,
                                ClientEndpoint = request.endPoint,
                                LastSyncTeeTimeMappingId = maxSyncTeeTimeMappingId
                            };

                            _context.SyncClients.Add(newSyncClient);
                            await _context.SaveChangesAsync();
                            int SyncClientId = newSyncClient.SyncClientId;

                            // 인증 생성
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


        public async Task<IDataResult> GetAuthentication(AuthenticationRequest request, string token)
        {
            if (string.IsNullOrEmpty(token) || token != AuthToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            try
            {
                var authType = request.authType;
                

                if (authType == "1") // 공급사
                {
                    var existingSupplierQuery = _context.Suppliers.Include(s => s.Authentication).Where(s => s.Authentication.Deleted == false);

                    if (request.authCode != null)
                    {
                        existingSupplierQuery = existingSupplierQuery.Where(s => s.SupplierCode == request.authCode);
                    }

                    var suppliers = new List<OAPI_Supplier>();
                    suppliers = await existingSupplierQuery.ToListAsync();

                    var response = suppliers.Select(supplier => new authAuthenticationResponse
                    {
                        authCode = supplier.SupplierCode,
                        TokenSupplier = supplier.Authentication.TokenSupplier,
                        TokenClient = supplier.Authentication.TokenClient,
                        AglCode = supplier.Authentication.AglCode,
                        TokenAgl = supplier.Authentication.TokenAgl,
                    }).ToList();

                    return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "Authentication List successfully", response);

                }
                else if (authType == "2") // 클라이언트
                {
                    var existingClientQuery = _context.SyncClients.Include(s => s.Authentication).Where(s => s.Authentication.Deleted == false);

                    if (request.authCode != null)
                    {
                        existingClientQuery = existingClientQuery.Where(s => s.ClientCode == request.authCode);
                    }

                    var syncClients = new List<OAPI_SyncClient>();
                    syncClients = await existingClientQuery.ToListAsync();

                    var response = syncClients.Select(syncClient => new authAuthenticationResponse
                    {
                        authCode = syncClient.ClientCode,
                        TokenSupplier = syncClient.Authentication.TokenSupplier,
                        TokenClient = syncClient.Authentication.TokenClient,
                        AglCode = syncClient.Authentication.AglCode,
                        TokenAgl = syncClient.Authentication.TokenAgl,
                    }).ToList();

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

        // AGLCODE 생성을 위한 로직
        public class PersistentAglCodeGenerator
        {
            private readonly string _filePath;

            public PersistentAglCodeGenerator(string filePath)
            {
                _filePath = filePath;

                // 폴더가 존재하지 않으면 생성
                string directoryPath = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }

            public string GetNextAglCode()
            {
                int currentCode = 1;

                if (File.Exists(_filePath))
                {
                    int.TryParse(File.ReadAllText(_filePath), out currentCode);
                }

                currentCode++;
                File.WriteAllText(_filePath, currentCode.ToString());

                return $"AGL{currentCode:D4}";
            }
        }


        //public async Task<IDataResult> CheckAuthentication(CheckAuthenticationRequest request)
        //{
        //    try
        //    {
        //        var syncClient = await _context.SyncClients.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.ClientCode == request.ClientCode);

        //        if (syncClient == null)
        //        {
        //            return await _commonService.CreateResponse<object>(false, ResultCode.UNAUTHORIZED, "client not found", null);
        //        }

        //        var tokenClient = syncClient.Authentication.TokenClient;

        //        if (tokenClient == null)
        //        {
        //            return await _commonService.CreateResponse<object>(false, ResultCode.UNAUTHORIZED, "clientToken not found", null);
        //        }

        //        var hashToken = ComputeSha256.ComputeSha256Hash(tokenClient);

        //        if(hashToken != request.token)
        //        {
        //            return await _commonService.CreateResponse<object>(false, ResultCode.UNAUTHORIZED, "signature Unauthorized", null);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Utils.UtilLogs.LogRegHour(request.ClientCode, request.ClientCode, $"Authentication", $"인증 실패 {ex.Message}", true);
        //        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
        //    }

        //}

    }
}
