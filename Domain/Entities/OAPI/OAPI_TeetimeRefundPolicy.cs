using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimeRefundPolicy
    {
        [Key]
        [Required]
        public int RefundPolicyId { get; set; }


        // 첫 번째 환불 조건
        public int? RefundDate_1 { get; set; } // 환불잔여일_1
        [StringLength(4)]
        public string? RefundHour_1 { get; set; } // 환불시간_1
        public decimal? RefundFee_1 { get; set; } // 환불수수료_1
        public byte? RefundUnit_1 { get; set; } // 환불단위_1

        // 두 번째 환불 조건
        public int? RefundDate_2 { get; set; } // 환불잔여일_2
        [StringLength(4)]
        public string? RefundHour_2 { get; set; } // 환불시간_2
        public decimal? RefundFee_2 { get; set; } // 환불수수료_2
        public byte? RefundUnit_2 { get; set; } // 환불단위_2

        // 세 번째 환불 조건
        public int? RefundDate_3 { get; set; } // 환불잔여일_3
        [StringLength(4)]
        public string? RefundHour_3 { get; set; } // 환불시간_3
        public decimal? RefundFee_3 { get; set; } // 환불수수료_3
        public byte? RefundUnit_3 { get; set; } // 환불단위_3

        // 네 번째 환불 조건
        public int? RefundDate_4 { get; set; } // 환불잔여일_4
        [StringLength(4)]
        public string? RefundHour_4 { get; set; } // 환불시간_4
        public decimal? RefundFee_4 { get; set; } // 환불수수료_4
        public byte? RefundUnit_4 { get; set; } // 환불단위_4

        // 다섯 번째 환불 조건
        public int? RefundDate_5 { get; set; } // 환불잔여일_5
        [StringLength(4)]
        public string? RefundHour_5 { get; set; } // 환불시간_5
        public decimal? RefundFee_5 { get; set; } // 환불수수료_5
        public byte? RefundUnit_5 { get; set; } // 환불단위_5

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();

        // 새로운 RefundDetails 프로퍼티 추가 (NotMapped)
        [NotMapped]
        public List<RefundDetail> RefundDetails
        {
            get
            {
                return new List<RefundDetail>
                {
                    new RefundDetail { RefundDate = RefundDate_1, RefundHour = RefundHour_1, RefundFee = RefundFee_1, RefundUnit = RefundUnit_1 },
                    new RefundDetail { RefundDate = RefundDate_2, RefundHour = RefundHour_2, RefundFee = RefundFee_2, RefundUnit = RefundUnit_2 },
                    new RefundDetail { RefundDate = RefundDate_3, RefundHour = RefundHour_3, RefundFee = RefundFee_3, RefundUnit = RefundUnit_3 },
                    new RefundDetail { RefundDate = RefundDate_4, RefundHour = RefundHour_4, RefundFee = RefundFee_4, RefundUnit = RefundUnit_4 },
                    new RefundDetail { RefundDate = RefundDate_5, RefundHour = RefundHour_5, RefundFee = RefundFee_5, RefundUnit = RefundUnit_5 }
                }.Where(rd => rd.RefundDate.HasValue || rd.RefundFee.HasValue || rd.RefundUnit.HasValue).ToList();
            }
            set
            {
                for (int i = 1; i <= 5; i++)
                {
                    var detail = value.ElementAtOrDefault(i - 1);
                    SetValue($"RefundDate_{i}", detail?.RefundDate);
                    SetValue($"RefundHour_{i}", detail?.RefundHour);
                    SetValue($"RefundFee_{i}", detail?.RefundFee);
                    SetValue($"RefundUnit_{i}", detail?.RefundUnit);
                }
            }
        }

        private void SetValue<T>(string propertyName, T? value) where T : struct
        {
            var property = GetType().GetProperty(propertyName);
            if (property != null)
                property.SetValue(this, value);
        }

        private void SetValue(string propertyName, string? value)
        {
            var property = GetType().GetProperty(propertyName);
            if (property != null)
                property.SetValue(this, value);
        }

    }

    public class RefundDetail
    {
        public int? RefundDate { get; set; }
        [StringLength(4)]
        public string? RefundHour { get; set; }
        public decimal? RefundFee { get; set; }
        public byte? RefundUnit { get; set; }
    }
}
