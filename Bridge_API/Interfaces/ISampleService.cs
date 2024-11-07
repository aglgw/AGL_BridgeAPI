using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Queries;
using AGL.Api.Infrastructure.Data;

namespace AGL.Api.Bridge_API.Interfaces
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