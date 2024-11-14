namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IDataResult
    {
        bool isSuccess { get; }
        string rstCd { get; }
        int statusCode { get; }
        string rstMsg { get; set; }        
    }
}
