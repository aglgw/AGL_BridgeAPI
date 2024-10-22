using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models;
using AGL.Api.ApplicationCore.Models.Queries;

namespace AGL.Api.ApplicationCore.Infrastructure
{
    public abstract class BaseService
    {
        public BaseService()
        {
            
        }
        protected IDataResult Successed(object data)
        {
            var result = new Success<dynamic>
            {
                Data = data
            };

            return result;
        }

        protected IDataResult Successed()
        {
            var result = new Success<dynamic>();
            return result;
        }


    }
}
