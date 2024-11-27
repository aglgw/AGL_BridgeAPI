using AGL.Api.Domain.Entities.OAPI;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimePricePolicy
    {
        [Key]
        [Required]
        public int PricePolicyId { get; set; }

        // 1인일 때 요금 정보
        public decimal? GreenFee_1 { get; set; } // 그린피_1
        public decimal? CartFee_1 { get; set; } // 카트피_1
        public decimal? CaddyFee_1 { get; set; } // 캐디피_1
        public decimal? Tax_1 { get; set; } // 세금_1
        public decimal? AdditionalTax_1 { get; set; } // 추가 세금_1
        public decimal? UnitPrice_1 { get; set; } // 1인 총요금_1

        // 2인일 때 요금 정보
        public decimal? GreenFee_2 { get; set; } // 그린피_2
        public decimal? CartFee_2 { get; set; } // 카트피_2
        public decimal? CaddyFee_2 { get; set; } // 캐디피_2
        public decimal? Tax_2 { get; set; } // 세금_2
        public decimal? AdditionalTax_2 { get; set; } // 추가 세금_2
        public decimal? UnitPrice_2 { get; set; } // 1인 총요금_2

        // 3인일 때 요금 정보
        public decimal? GreenFee_3 { get; set; } // 그린피_3
        public decimal? CartFee_3 { get; set; } // 카트피_3
        public decimal? CaddyFee_3 { get; set; } // 캐디피_3
        public decimal? Tax_3 { get; set; } // 세금_3
        public decimal? AdditionalTax_3 { get; set; } // 추가 세금_3
        public decimal? UnitPrice_3 { get; set; } // 1인 총요금_3

        // 4인일 때 요금 정보
        public decimal? GreenFee_4 { get; set; } // 그린피_4
        public decimal? CartFee_4 { get; set; } // 카트피_4
        public decimal? CaddyFee_4 { get; set; } // 캐디피_4
        public decimal? Tax_4 { get; set; } // 세금_4
        public decimal? AdditionalTax_4 { get; set; } // 추가 세금_4
        public decimal? UnitPrice_4 { get; set; } // 1인 총요금_4

        // 5인일 때 요금 정보
        public decimal? GreenFee_5 { get; set; } // 그린피_5
        public decimal? CartFee_5 { get; set; } // 카트피_5
        public decimal? CaddyFee_5 { get; set; } // 캐디피_5
        public decimal? Tax_5 { get; set; } // 세금_5
        public decimal? AdditionalTax_5 { get; set; } // 추가 세금_5
        public decimal? UnitPrice_5 { get; set; } // 1인 총요금_5


        // 6인일 때 요금 정보
        public decimal? GreenFee_6 { get; set; } // 그린피_5
        public decimal? CartFee_6 { get; set; } // 카트피_5
        public decimal? CaddyFee_6 { get; set; } // 캐디피_5
        public decimal? Tax_6 { get; set; } // 세금_5
        public decimal? AdditionalTax_6 { get; set; } // 추가 세금_5
        public decimal? UnitPrice_6 { get; set; } // 1인 총요금_5

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();

        [NotMapped]
        public List<PriceDetail> PriceDetails
        {
            get
            {
                // 데이터를 리스트로 변환
                return Enumerable.Range(1, 6) // PlayerCount 1~6 반복
                    .Select(i => new PriceDetail
                    {
                        PlayerCount = i,
                        GreenFee = GetValue($"GreenFee_{i}"),
                        CartFee = GetValue($"CartFee_{i}"),
                        CaddyFee = GetValue($"CaddyFee_{i}"),
                        Tax = GetValue($"Tax_{i}"),
                        AdditionalTax = GetValue($"AdditionalTax_{i}"),
                        UnitPrice = GetValue($"UnitPrice_{i}")
                    })
                    .Where(pd => pd.GreenFee.HasValue || pd.CartFee.HasValue || pd.CaddyFee.HasValue || pd.Tax.HasValue || pd.AdditionalTax.HasValue || pd.UnitPrice.HasValue)
                    .ToList();
            }
            set
            {
                // PriceDetails 데이터를 컬럼에 매핑
                foreach (var detail in value)
                {
                    SetValue($"GreenFee_{detail.PlayerCount}", detail.GreenFee);
                    SetValue($"CartFee_{detail.PlayerCount}", detail.CartFee);
                    SetValue($"CaddyFee_{detail.PlayerCount}", detail.CaddyFee);
                    SetValue($"Tax_{detail.PlayerCount}", detail.Tax);
                    SetValue($"AdditionalTax_{detail.PlayerCount}", detail.AdditionalTax);
                    SetValue($"UnitPrice_{detail.PlayerCount}", detail.UnitPrice);
                }
            }
        }

        private decimal? GetValue(string propertyName)
        {
            return (decimal?)GetType().GetProperty(propertyName)?.GetValue(this);
        }

        private void SetValue(string propertyName, decimal? value)
        {
            var property = GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(this, value);
            }
        }
    }

    public class PriceDetail
    {
        public int PlayerCount { get; set; } // 플레이어 수
        public decimal? GreenFee { get; set; } // 그린피
        public decimal? CartFee { get; set; } // 카트피
        public decimal? CaddyFee { get; set; } // 캐디피
        public decimal? Tax { get; set; } // 세금
        public decimal? AdditionalTax { get; set; } // 추가세금
        public decimal? UnitPrice { get; set; } // 1인 총요금
    }
}