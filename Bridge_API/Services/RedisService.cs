using StackExchange.Redis;

namespace AGL.Api.Bridge_API.Services
{
    public class RedisService
    {
        private static Lazy<ConnectionMultiplexer> _redisConnection;
        private readonly IConfiguration _configuration;

        public RedisService(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_redisConnection == null || !_redisConnection.IsValueCreated)
            {
                InitializeConnection();
            }
        }

        private void InitializeConnection()
        {
            _redisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var config = new ConfigurationOptions
                {
                    EndPoints = { $"{_configuration["Redis:Host"]}:{_configuration["Redis:Port"]}" },
                    Password = _configuration["Redis.Password"],
                    AbortOnConnectFail = false,
                    SyncTimeout = 10000, // 타임아웃 10초
                    AsyncTimeout = 10000, // 비동기 타임아웃
                    KeepAlive = 60 // KeepAlive 설정 (초 단위)
                };

                return ConnectionMultiplexer.Connect(config);
            });
        }

        public IDatabase GetDatabase() => _redisConnection.Value.GetDatabase();
    }
}