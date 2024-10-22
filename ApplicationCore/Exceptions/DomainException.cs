using AGL.Api.ApplicationCore.Models.Enum;
using System;

public class DomainException : Exception
{
    public ResultCode Code { get; set; }
    public DomainException(ResultCode code, string message) : base(message)
    {
        this.Code = code;
    }
    public DomainException(ResultCode code, string message, Exception innerException) : base(message, innerException)
    {
        this.Code = code;
    }
}
