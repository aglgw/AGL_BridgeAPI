using AGL.Api.ApplicationCore.Interfaces;

namespace AGL.Api.API_Schedulers.Interfaces
{
    public interface ISyncTeeTimeService
    {
        Task<IDataResult> SyncTeeTime();
    }
}
