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
using AGL.Api.Domain.Entities.OAPI;
using Azure.Core;

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
            try
            {
                UtilLogs.LogRegHour("SyncTeeTime", $"SyncTeeTime", $"동기화 프로세스 시작", "");
                // 동기화가 필요한 모든 클라이언트를 가져옵니다
                var clients = await _context.SyncClients.ToListAsync();

                var tasks = clients.Select(async client =>
                {
                    try
                    {
                        UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName : {client.ClientName} 동기화 시작", "");

                        var syncTeeTimeMappings = await _context.SyncTeeTimeMappings
                            .Where(stm => stm.SyncTeeTimeMappingId > client.LastSyncTeeTimeMappingId)
                            .Distinct()
                            .Select(stm => new
                            {
                                stm.SyncTeeTimeMappingId,
                                TeeTime = new
                                {
                                    stm.TeeTimeMapping.TeeTime.Supplier.DaemonId,
                                    stm.TeeTimeMapping.TeeTime.GolfClub.GolfClubCode,
                                    stm.TeeTimeMapping.TeeTime.GolfClubCourse.CourseCode,
                                    stm.TeeTimeMapping.TeeTime.MinMembers
                                },
                                DateSlot = new
                                {
                                    stm.TeeTimeMapping.DateSlot.PlayDate
                                },
                                TimeSlot = new
                                {
                                    stm.TeeTimeMapping.TimeSlot.StartTime
                                },
                                TeetimePricePolicy = new
                                {
                                    stm.TeeTimeMapping.TeetimePricePolicy.UnitPrice_4
                                },
                                stm.TeeTimeMapping.IsAvailable
                            })
                            .ToListAsync();

                        // teeTimeMappings가 있을 때만 아래 코드를 실행합니다
                        if (syncTeeTimeMappings.Any())
                        {
                            UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName: {client.ClientName}에 대해 {syncTeeTimeMappings.Count}개의 티타임 매핑을 준비", "");
                            // 클라이언트에 전송할 객체 리스트를 준비합니다
                            var syncTeeTimeRequests = syncTeeTimeMappings.Select(stm => 
                            {
                                // 클라이언트 마다 객체 모양 다르게 설정
                                ISyncTeeTimeRequest request;
                                if (client.ClientName == "ClientA")
                                {
                                    request = new SyncTeeTimeRequest
                                    {
                                        daemonId = stm.TeeTime.DaemonId,
                                        golfClubCode = stm.TeeTime.GolfClubCode,
                                        playDate = stm.DateSlot.PlayDate,
                                        startTime = stm.TimeSlot.StartTime,
                                        price = stm.TeetimePricePolicy.UnitPrice_4
                                    };
                                }
                                else
                                {
                                    request = new SyncTeeTimeRequest
                                    {
                                        daemonId = stm.TeeTime.DaemonId,
                                        golfClubCode = stm.TeeTime.GolfClubCode,
                                        courseCode = stm.TeeTime.CourseCode,
                                        playDate = stm.DateSlot.PlayDate,
                                        startTime = stm.TimeSlot.StartTime,
                                        price = stm.TeetimePricePolicy.UnitPrice_4,
                                        minMembers = stm.TeeTime.MinMembers,
                                        IsAvailable = stm.IsAvailable
                                    };
                                }
                                return request;
                            }).ToList();

                            // 동기화할 티타임 요청이 있는 경우, 클라이언트의 엔드포인트로 전송합니다
                            if (syncTeeTimeRequests.Count > 0)
                            {
                                UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName: {client.ClientName}로 데이터 전송 시작", "");
#if DEBUG
                                var success = true;
#else
                                var success = await SendDataToClient(_httpClient, client, syncTeeTimeRequests);
#endif
                                if (success)
                                {
                                    // 동기화가 성공하면 클라이언트의 LastSyncTeeTimeMappingId를 업데이트합니다
                                    client.LastSyncTeeTimeMappingId = syncTeeTimeMappings.Max(stm => stm.SyncTeeTimeMappingId);
                                    UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName: {client.ClientName}의 LastSyncTeeTimeMappingId를 {client.LastSyncTeeTimeMappingId}로 업데이트", "");
                                    _context.SyncClients.Update(client);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName: {client.ClientName}로 데이터 전송 실패", "", true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 클라이언트 동기화 실패에 대한 오류를 기록합니다
                        UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"ClientName: {client.ClientName} 처리 실패: {ex.Message}", "", true);
                    }
                });
                // 모든 클라이언트 동기화 작업을 동시에 실행합니다
                await Task.WhenAll(tasks);
                UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", "동기화 프로세스 완료", "");
            }
            catch (Exception ex)
            {
                // 전체 동기화 프로세스에 대한 일반 오류를 기록합니다
                UtilLogs.LogRegHour("SyncTeeTime", "SyncTeeTime", $"전체 동기화 실패: {ex.Message}", "", true);
            }

            return Successed();
        }

        private async Task<bool> SendDataToClient(HttpClient httpClient, OAPI_SyncClient client, List<ISyncTeeTimeRequest> syncTeeTimeRequests)
        {
            try
            {
                UtilLogs.LogRegHour("SendDataToClient", "SendDataToClient", $"ClientName: {client.ClientName}로 데이터 전송 시작", "");
                // syncTeeTimeRequests를 직렬화하여 HTTP 요청 콘텐츠를 준비합니다
                var content = new StringContent(JsonSerializer.Serialize(syncTeeTimeRequests), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(client.ClientEndpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    UtilLogs.LogRegHour("SendDataToClient", "SendDataToClient", $"ClientName: {client.ClientName}로 데이터 전송 성공", "");
                }
                else
                {
                    UtilLogs.LogRegHour("SendDataToClient", "SendDataToClient", $"ClientName: {client.ClientName}로 데이터 전송 실패", "", true);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // 클라이언트로 데이터 전송 실패 시 오류를 기록합니다
                UtilLogs.LogRegHour("SyncTeeTime", $"", $"{client.ClientEndpoint}로 데이터 전송 실패: {ex.Message}", "", true);
                return false;
            }
        }

    }
}
