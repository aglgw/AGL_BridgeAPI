using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Newtonsoft.Json;

namespace AGL.Api.ApplicationCore.Models
{
    public abstract class BaseResult : IDataResult
    {
        [JsonIgnore]
        public ResultCode Code { get; set; }

        public bool isSuccess
        {
            get
            {
                return Code == ResultCode.SUCCESS ? true : false;
            }
        }

        [JsonProperty(Order = 0)]
        public string rstCd
        {
            get
            {
                return Code.Description();
            }
        }
        public int statusCode
        {
            get
            {
                return (int)Code;
            }
        }
        [JsonProperty(Order = 1)]
        public string rstMsg { get; set; } = string.Empty;
    }
}
