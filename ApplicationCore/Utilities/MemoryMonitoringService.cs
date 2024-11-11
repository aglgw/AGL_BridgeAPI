using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Utilities
{
    public class MemoryMonitoringService : BackgroundService
    {
        private readonly ILogger<MemoryMonitoringService> _logger;
        private readonly IOptions<AppSettings> _settings;

        public MemoryMonitoringService(ILogger<MemoryMonitoringService> logger, IOptions<AppSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 현재 실행 중인 프로세스를 가져옵니다.
                Process currentProcess = Process.GetCurrentProcess();

                // 작업 세트 메모리 사용량 (현재 사용 중인 실제 물리 메모리)
                long workingSet = currentProcess.WorkingSet64;

                // 메모리 사용량에 따른 GC 설정 조정
                if (workingSet >= _settings.Value.LimitMemory * 1024 * 1024)
                {
                    // 서버 GC 모드 활성화 및 힙 메모리 제한 설정
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                // 주기적으로 메모리 상태 확인 (1분 간격)
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // AppSettings 클래스 접근 수준을 public으로 변경
        public class AppSettings
        {
            public int LimitMemory { get; set; }
        }
    }

}
