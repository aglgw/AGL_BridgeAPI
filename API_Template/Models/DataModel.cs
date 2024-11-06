namespace AGL.Api.Bridge_API.Models
{
    public record ShopList
    {
        public string ShopId { get; set; } = string.Empty;
        public string ShopCode { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string? ShopNameEng { get; set; }
        public string? WareHouseCode { get; set; }
        public int Status { get; set; }
        public int TableCnt { get; set; }
        public string ShopKind { get; set; } = string.Empty;

    }
}
