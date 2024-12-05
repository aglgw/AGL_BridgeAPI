using AGL.Api.ApplicationCore.Interfaces;
using StackExchange.Redis;

namespace AGL.Api.Bridge_API.Services
{
    public class RedisService : IRedisService
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

        public IDatabase GetDatabase()
        {
            return _redisConnection.Value.GetDatabase();
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            var db = GetDatabase();
            return await db.KeyExistsAsync(key);
        }

        public async Task<string> GetValueAsync(string key)
        {
            var db = GetDatabase();
            return await db.StringGetAsync(key);
        }

        public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null)
        {
            var db = GetDatabase();
            await db.StringSetAsync(key, value, expiry);
        }
    }
}