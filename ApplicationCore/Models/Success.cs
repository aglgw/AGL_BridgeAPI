using AGL.Api.ApplicationCore.Models.Enum;
using Newtonsoft.Json;

namespace AGL.Api.ApplicationCore.Models
{
    public class Success<T> : BaseResult 
        where T: class
    {
        public Success()
        {
            Code = ResultCode.성공;
        }
        [JsonProperty(Order = 3)]
        public T Data { get; set; }
    }
}
