using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Utilities;
using AGL.Api.Schedulers_API.Interfaces;
using AGL.Api.Schedulers_API.Models;
using AGL.Api.Infrastructure.Data;
using StoredProcedureEFCore;
using System.Net.NetworkInformation;
using AGL.Api.API_Schedulers.Interfaces;
using System.Text;
using AGL.Api.API_Schedulers.Models;
using static AGL.Api.API_Schedulers.Models.OAPI.OAPIScheduler;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using AGL.Api.Schedulers_API.Utils;
using Microsoft.AspNetCore.WebUtilities;
using System.Runtime.Intrinsics.Arm;

namespace AGL.Api.API_Schedulers.Services
{
    public class SyncTeeTimeService : BaseService , ISyncTeeTimeService
    {
        private readonly OAPI_DbContext _context;
        private readonly HttpClient _httpClient;

        public SyncTeeTimeService(OAPI_DbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<IDataResult> SyncTeeTime()
        {
            var suppliers = await _context.Suppliers.ToListAsync();

            try
            {
                // 동기화가 필요한 모든 클라이언트를 가져옵니다
                var clients = await _context.SyncClients.ToListAsync();
                var tasks = clients.Select(async client =>
                {
                    try
                    {
                        UtilLogs.LogRegHour("SyncTeeTime", $"", $"싱크 검색 시작", "", true);
                        // 현재 클라이언트에 대해 동기화가 필요한 모든 티타임 매핑을 가져옵니다
                        var syncTeeTimeMappings = await _context.SyncTeeTimeMappings
                            .Where(stm => stm.SyncTeeTimeMappingId > client.LastSyncTeeTimeMappingId)
                            .Include(stm => stm.TeeTimeMapping)  // TeetimeMapping 네비게이션 속성을 포함합니다
                                .ThenInclude(tm => tm.TeeTime) // TeeTime 세부 정보를 포함합니다
                            .Include(stm => stm.TeeTimeMapping.DateSlot) // DateSlot 세부 정보를 포함합니다
                            .Include(stm => stm.TeeTimeMapping.TimeSlot) // TimeSlot 세부 정보를 포함합니다
                            .Include(stm => stm.TeeTimeMapping.TeetimePricePolicy) // PricePolicy 세부 정보를 포함합니다
                            .ToListAsync();

                        // teeTimeMappings가 있을 때만 아래 코드를 실행합니다
                        if (syncTeeTimeMappings.Any())
                        {
                            // 클라이언트에 전송할 SyncTeeTimeRequest 객체 리스트를 준비합니다
                            var syncTeeTimeRequests = syncTeeTimeMappings.Select(stm => new SyncTeeTimeRequest
                            {
                                daemonId = stm.TeeTimeMapping.TeeTime.Supplier.DaemonId,
                                golfClubCode = stm.TeeTimeMapping.TeeTime.GolfClub.GolfClubCode,
                                courseCode = stm.TeeTimeMapping.TeeTime.GolfClubCourse.CourseCode,
                                playDate = stm.TeeTimeMapping.DateSlot.PlayDate,
                                startTime = stm.TeeTimeMapping.TimeSlot.StartTime,
                                price = stm.TeeTimeMapping.TeetimePricePolicy.UnitPrice_4,
                                minMembers = stm.TeeTimeMapping.TeeTime.MinMembers,
                                IsAvailable = stm.TeeTimeMapping.IsAvailable
                            }).ToList();

                            // 동기화할 티타임 요청이 있는 경우, 클라이언트의 엔드포인트로 전송합니다
                            if (syncTeeTimeRequests.Count > 0)
                            {
                                //var success = await SendDataToClient(_httpClient, client.ClientEndpoint, syncTeeTimeRequests);
                                var success = true;
                                if (success)
                                {
                                    // 동기화가 성공하면 클라이언트의 LastSyncTeeTimeMappingId를 업데이트합니다
                                    client.LastSyncTeeTimeMappingId = syncTeeTimeMappings.Max(stm => stm.SyncTeeTimeMappingId);
                                    _context.SyncClients.Update(client);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 클라이언트 동기화 실패에 대한 오류를 기록합니다
                        UtilLogs.LogRegHour("SyncTeeTime", $"", $"fail response : {ex.Message}","", true);
                        Console.WriteLine($"클라이언트 {client.ClientName} 처리 실패: {ex.Message}");
                    }
                });
                // 모든 클라이언트 동기화 작업을 동시에 실행합니다
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // 전체 동기화 프로세스에 대한 일반 오류를 기록합니다
                UtilLogs.LogRegHour("SyncTeeTime", $"", $"clients error : {ex.Message}", "", true);
                Console.WriteLine($"티타임 동기화 실패: {ex.Message}");
            }

            return Successed();
        }

        private async Task<bool> SendDataToClient(HttpClient httpClient, string clientEndpoint, List<SyncTeeTimeRequest> syncTeeTimeRequests)
        {
            try
            {
                // syncTeeTimeRequests를 직렬화하여 HTTP 요청 콘텐츠를 준비합니다
                var content = new StringContent(JsonSerializer.Serialize(syncTeeTimeRequests), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(clientEndpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // 클라이언트로 데이터 전송 실패 시 오류를 기록합니다
                UtilLogs.LogRegHour("SyncTeeTime", $"", $"{clientEndpoint}로 데이터 전송 실패: {ex.Message}", "", true);
                Console.WriteLine($"{clientEndpoint}로 데이터 전송 실패: {ex.Message}");
                return false;
            }
        }

    }
}
