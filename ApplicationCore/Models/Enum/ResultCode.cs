using System.ComponentModel;

namespace AGL.Api.ApplicationCore.Models.Enum
{
    public enum ResultCode
    {
        [Description("SUCCESS")]
        SUCCESS = 200, //성공
        [Description("INVALID_INPUT")]
        INVALID_INPUT = 400, //잘못된요청
        [Description("UNAUTHORIZED")]
        UNAUTHORIZED = 401, //비승인
        [Description("FORBIDDEN")]
        Forbidden = 403, //금지
        [Description("NOT_FOUND")]
        NOT_FOUND = 404, //못찾음
        [Description("SERVER_ERROR")]
        SERVER_ERROR = 500 //시스템오류
    }
}
