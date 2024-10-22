using System.ComponentModel;

namespace AGL.Api.ApplicationCore.Models.Enum
{
    public enum ResultCode
    {
        [Description("SC00")]
        SUCCESS = 200, //성공
        [Description("FL01")]
        INVALID_INPUT = 400, //잘못된요청
        [Description("FL00")]
        UNAUTHORIZED = 401, //비승인
        [Description("FL02")]
        Forbidden = 403, //금지
        [Description("FL02")]
        NOT_FOUND = 404, //못찾음
        [Description("ET00")]
        SERVER_ERROR = 500 //시스템오류
    }
}
