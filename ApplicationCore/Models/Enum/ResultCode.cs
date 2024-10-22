using System.ComponentModel;

namespace AGL.Api.ApplicationCore.Models.Enum
{
    public enum ResultCode
    {
        [Description("SC00")]
        성공 = 0,
        [Description("FL00")]
        데이터없음,
        [Description("FL01")]
        잘못된요청,
        [Description("FL02")]
        비로그인,
        [Description("ET00")]
        시스템오류
    }
}
