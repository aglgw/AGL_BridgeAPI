﻿using AGL.Api.ApplicationCore.Utilities;
using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Infrastructure.Data;
using Azure;
using System.Diagnostics;
using System.Text.Json;

namespace AGL.Api.Bridge_API.Services
{
    public class BackgroundRequestService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RequestQueue _queue;
        private readonly ILogger<BackgroundRequestService> _logger;
        private Task _backgroundTask;
        private CancellationTokenSource _stoppingCts;

        public BackgroundRequestService(IServiceScopeFactory scopeFactory, RequestQueue queue, ILogger<BackgroundRequestService> logger)
        {
            _scopeFactory = scopeFactory;
            _queue = queue;
            _logger = logger;
            _stoppingCts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _backgroundTask = Task.Run(() => ExecuteAsync(_stoppingCts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_backgroundTask == null)
            {
                return;
            }

            _stoppingCts.Cancel();
            await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var request))
                {
                    Utils.UtilLogs.LogRegHour(request.supplierCode, request.golfClubCode, $"TeeTime queue start", $"TeeTime queue start");

                    var directory = Path.Combine("C:", "AGL", "JSON", "TEETIME");
                    var fileNameFormat = $"Request_{request.supplierCode}_{request.golfClubCode}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid()}.json";

                    var fileName = await Util.SaveJsonToFileAsync(directory, fileNameFormat, request, stoppingToken);

                    Utils.UtilLogs.LogRegHour(request.supplierCode, request.golfClubCode, $"TeeTime queue json", $"json saved to file: {fileName}");

                    using var scope = _scopeFactory.CreateScope();
                    var teeTimeService = scope.ServiceProvider.GetRequiredService<TeeTimeService>();
                    await teeTimeService.ProcessTeeTime(request, request.supplierCode, request.golfClubCode);

                    Utils.UtilLogs.LogRegHour(request.supplierCode, request.golfClubCode, $"TeeTime queue end", $"TeeTime queue end");
                }
                await Task.Delay(2000, stoppingToken); // 큐 딜레이 설정
            }
        }
    }
}
