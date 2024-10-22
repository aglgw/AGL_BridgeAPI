namespace AGL.Api.HTT.Models
{
    public record testModel
    {
        public int idx {  get; set; }
        public string name { get; set; }
        public List<testModelDtl> Dtl { get; set; }

    }

    public record testModelDtl
    {
        public int dtlIdx { get; set; }
        public string name { get; set; }
        public DateTime regDt { get; set; }
        public DateTime? modDt { get; set; }
    }

    public record testModel_2
    {
        public int Idx { get; set; }
        public string Name { get; set; }
        public int? DtlIdx { get; set; }
        public string? DtlName { get; set; }
        public DateTime? DtlRegDt { get; set; }
        public DateTime? DtlModDt { get; set; }
    }

    public record LanguageMappingByNation
    {
        public string nationCd { get; set; } = string.Empty;
        public string langCd { get; set; } = string.Empty;

    }
}
