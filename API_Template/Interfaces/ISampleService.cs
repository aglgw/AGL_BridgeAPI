using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Queries;
using AGL.Api.Infrastructure.Data;

namespace AGL.Api.API_Template.Interfaces
{

    public interface ISampleService
    {
        Task<IDataResult> GetCheckInTeeTimeList();
        Task<IDataResult> CallSp(string fieldId);
        Task<IDataResult> GetMaria();
        Task<IDataResult> GetHTT();


        Task<IDataResult> SetMariaInsertTest(CancellationToken cancellationToken);
        Task<IDataResult> SetMariaUpdateTest(CancellationToken cancellationToken);

        Task<IDataResult> DelMariaDeleteTest(CancellationToken cancellationToken);
        Task<IDataResult> GetMariaMultiSelectSample();

        Task<IDataResult> GetMariaSubEntitySelectSample();
        Task<IDataResult> GetMariaJoinSelectSample();

        Task<IDataResult> GetMariaSelectExpressionSample();


        Task<IDataResult> GetMariaSelectQueryFilterSample();

        Task<IDataResult> GetMariaSelectPagingSample(DomainQuery query);


        Task<IDataResult> SetMariaBulkSample();

        Task MigNation();


    }
}