using AGL.Api.ApplicationCore.Utilities;
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
            //_logger.LogInformation("BackgroundRequestService is starting.");
            Debug.WriteLine("BackgroundRequestService is starting.");

            _backgroundTask = Task.Run(() => ExecuteAsync(_stoppingCts.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_backgroundTask == null)
            {
                return;
            }
            //_logger.LogInformation("BackgroundRequestService is stopping.");
            Debug.WriteLine("BackgroundRequestService is stopping.");

            _stoppingCts.Cancel();
            await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("BackgroundRequestService is running.");
            Debug.WriteLine("BackgroundRequestService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var request))
                {
                    //_logger.LogInformation("Processing request for SupplierCode: {SupplierCode}, GolfclubCode: {GolfclubCode}", request.SupplierCode, request.GolfclubCode);
                    Debug.WriteLine($"Processing request for SupplierCode: {request.SupplierCode}, GolfclubCode: {request.GolfclubCode}");

                    var directory = "C:\\AGL\\JSON";
                    Directory.CreateDirectory(directory);
                    // Save request to a JSON file
                    var json = JsonSerializer.Serialize(request);
                    var fileName = Path.Combine(directory, $"Request_{request.SupplierCode}_{request.GolfclubCode}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.json");
                    await File.WriteAllTextAsync(fileName, json, stoppingToken);

                    //_logger.LogInformation("Request saved to file: {FileName}", fileName);
                    Debug.WriteLine($"Request saved to file: {fileName}");

                    using var scope = _scopeFactory.CreateScope();
                    var teeTimeService = scope.ServiceProvider.GetRequiredService<TeeTimeService>();
                    await teeTimeService.ProcessTeeTime(request, request.SupplierCode, request.GolfclubCode);
                }
                await Task.Delay(1000, stoppingToken); // Adjust delay as needed
            }

            //_logger.LogInformation("BackgroundRequestService is stopping.");
            Debug.WriteLine("BackgroundRequestService has stopped.");
        }
    }
}
