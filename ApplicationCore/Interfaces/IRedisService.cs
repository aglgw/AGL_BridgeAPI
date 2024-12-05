namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IRedisService
    {
        Task<bool> KeyExistsAsync(string key);
        Task<string> GetValueAsync(string key);
        Task SetValueAsync(string key, string value, TimeSpan? expiry = null);
    }
}
