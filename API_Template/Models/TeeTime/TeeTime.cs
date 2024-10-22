namespace AGL.Api.API_Template.Models.TeeTime
{
    public class TeeTime
    {
        public class OAPI_TeeTime
        {
            public int TeetimeId { get; set; } // PK
            public int SupplierId { get; set; } // FK
            public int GolfClubId { get; set; } // FK
            public int GolfClubCourseId { get; set; } // FK
            public int MinMembers { get; set; }
            public int MaxMembers { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }

            public virtual ICollection<OAPI_DateTimeMapping> DateTimeMappings { get; set; } = new List<OAPI_DateTimeMapping>();
            public virtual ICollection<OAPI_TeetimePriceMapping> PriceMappings { get; set; } = new List<OAPI_TeetimePriceMapping>();
            public virtual ICollection<OAPI_TeetimeRefundMapping> RefundMappings { get; set; } = new List<OAPI_TeetimeRefundMapping>();
        }

        public class OAPI_TeetimeRefundMapping
        {
            public int DateTimeMappingId { get; set; } // FK
            public int RefundPolicyId { get; set; } // FK

            public virtual OAPI_DateTimeMapping DateTimeMapping { get; set; }
            public virtual OAPI_TeetimeRefundPolicy RefundPolicy { get; set; }
        }

        public class OAPI_DateSlot
        {
            public int DateSlotId { get; set; } // PK
            public string PlayDate { get; set; } // CHAR(8)

        }

        public class OAPI_TimeSlot
        {
            public int TimeSlotId { get; set; } // PK
            public string StartTime { get; set; } // CHAR(4)
        }

        public class OAPI_DateTimeMapping
        {
            public int DateTimeMappingId { get; set; } // PK
            public int? TeetimeId { get; set; } // FK
            public int? DateSlotId { get; set; } // FK
            public int? TimeSlotId { get; set; } // FK
            public string SupplierTeetimeCode { get; set; }
            public byte Isavailable { get; set; } // TINYINT
            public byte IsDisable { get; set; } // TINYINT
            public byte IsDeleted { get; set; } // TINYINT
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }

            public virtual OAPI_TeeTime TeeTime { get; set; }
        }

        public class OAPI_TeetimePriceMapping
        {
            public int DateTimeMappingId { get; set; } // FK
            public int PricePolicyId { get; set; } // FK

            public virtual OAPI_DateTimeMapping DateTimeMapping { get; set; }
            public virtual OAPI_PricePolicy PricePolicy { get; set; }
        }

        public class OAPI_PricePolicy
        {
            public int PricePolicyId { get; set; } // PK
            public int PlayerCount { get; set; }
            public decimal GreenFee { get; set; }
            public decimal? CartFee { get; set; }
            public decimal? CaddyFee { get; set; }
            public decimal? Tax { get; set; }
            public decimal? AdditionalTax { get; set; }
            public decimal UnitPrice { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }

            public virtual ICollection<OAPI_TeetimePriceMapping> PriceMappings { get; set; } = new List<OAPI_TeetimePriceMapping>();
        }


        public class OAPI_TeetimeRefundPolicy
        {
            public int RefundPolicyId { get; set; } // PK
            public string RefundDate { get; set; } // CHAR(8)
            public string RefundHour { get; set; } // CHAR(4)
            public decimal RefundFee { get; set; }
            public byte RefundUnit { get; set; } // TINYINT

            public virtual ICollection<OAPI_TeetimeRefundMapping> TeetimeRefundMappings { get; set; } = new List<OAPI_TeetimeRefundMapping>();
        }
    }
}
