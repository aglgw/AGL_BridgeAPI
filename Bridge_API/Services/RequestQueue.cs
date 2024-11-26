using System.Collections.Concurrent;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;

namespace AGL.Api.Bridge_API.Services
{
    public class RequestQueue
    {
        private readonly ConcurrentQueue<TeeTimeBackgroundRequest> _queue = new();
        public void Enqueue(TeeTimeBackgroundRequest request) => _queue.Enqueue(request);
        public bool TryDequeue(out TeeTimeBackgroundRequest request) => _queue.TryDequeue(out request);
    }
}
