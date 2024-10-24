using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;

namespace AGL.Api.API_Template.Services
{
    public class OAPIService : BaseService, IOAPIService
    {
        public Task<OAPIResponse.OAPITeeTimeGetResponse> GetTeeTime(OAPITeeTimeGetRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult> PostReservatioConfirm(OAPIReservationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult> PostTeeTime(OAPITeeTimePostRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult> PostTeeTimeAvailability(OAPITeeTimetAvailabilityRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult> PutTeeTime(OAPITeeTimePutRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
