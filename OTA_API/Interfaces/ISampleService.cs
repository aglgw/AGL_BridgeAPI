using AGL.Api.ApplicationCore.Interfaces;

namespace AGL.Api.OTA_API.Interfaces
{

    public interface ISampleService
    {
        Task<IDataResult> GetCheckInTeeTimeList();
        Task<IDataResult> CallSp(string fieldId);
        Task<IDataResult> GetHTT();
        Task<IDataResult> GetEnvironment();
        Task MigNation();


    }
}