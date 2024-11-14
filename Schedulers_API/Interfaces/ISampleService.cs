using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Infrastructure.Data;

namespace AGL.Api.Schedulers_API.Interfaces
{

    public interface ISampleService
    {
        Task<IDataResult> GetCheckInTeeTimeList();
        Task<IDataResult> CallSp(string fieldId);

        Task Test(string str);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> TestProtocol();

    }

}