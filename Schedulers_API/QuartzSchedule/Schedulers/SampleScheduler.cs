using AGL.Api.Schedulers_API.Jobs;
using Quartz;

namespace AGL.Api.Schedulers_API.Schedulers
{
    public class SampleScheduler
    {

        private readonly ILogger<SampleScheduler> _logger;
        private readonly IScheduler _scheduler;
        //private AppSettings _appSettings;

        public SampleScheduler(ILogger<SampleScheduler> logger,
            //IOptionsMonitor<AppSettings> optionsMonitor,
            IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
            //_appSettings = optionsMonitor.CurrentValue;
            //optionsMonitor.OnChange(config =>
            //{
            //    _appSettings = config;
            //});
        }

        // [초 분 시 일 월 요일 연도] Cron 표현식
        // * : 모든 값
        // ? : 특정 값 없음
        // - : 범위 지정
        // , : 여러 값 지정 구분에 사용
        // / : 초기값과 증가치 설정에 사용
        // L : 지정할 수 있는 범위의 마지막 값
        // W : 월~금요일 또는 가장 가까운 월/ 금요일
        // # : 몇 번째 무슨 요일 2#1 => 첫 번째 월요일
        // */10 * * * * ?       => 10초에 한번
        // * */15 * * * ?       => 15의 배수 분 동안 계속
        // 0 3/5 * * * ?        => 3분부터 5씩더한분 실행 3,8,13,18
        // 1 0 0 * * ?          => 매일 0시 0분 1초에 실행
        // 0 0/30 8-9 5,20 * ?  => 5,20일 08:00, 08:30, 09:00, 09:30 실행

        public Task SampleJob()
        {
            var jobDetails = JobBuilder
                .CreateForAsync<SampleJob>()
                .WithIdentity("SampleJob")
                .WithDescription("샘플 스케쥴")
                .Build();

            var trigger = TriggerBuilder
                .Create()
                .StartNow()
                .WithCronSchedule($"0/10 * * * * ?")  // 10초 마다 실행
                .Build();

            var result = _scheduler.ScheduleJob(jobDetails, trigger);
            return result;
        }

    }
}
