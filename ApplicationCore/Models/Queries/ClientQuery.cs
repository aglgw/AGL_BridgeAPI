namespace AGL.Api.ApplicationCore.Models.Queries
{
    public record ClientQuery
    {
        public string ClientId { get; init; }
        public int UserId { get; init; }
        public string Language { get; init; }
        public string Currency { get; init; }
    }
}
