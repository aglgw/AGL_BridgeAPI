using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities
{
    public class HTT_CODE_AREA
    {
        [Key]        
        public string AREA_CD { get; set; }        
        public string AREA_NM { get; set; }


        [JsonIgnore]
        public virtual ICollection<HTT_CODE_NATION> Nations { get; set; }

    }
}
