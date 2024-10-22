namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IDataResult
    {
        bool IsSuccess { get; }
        string RstCd { get; }
        int StatusCode { get; }
        string RstMsg { get; set; }        
    }
}
