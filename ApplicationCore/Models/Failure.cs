using Newtonsoft.Json;

namespace AGL.Api.ApplicationCore.Models
{
    public class Failure : BaseResult
    {
        [JsonProperty(Order = 3)]
        public object More { get; set; }
    }
}
