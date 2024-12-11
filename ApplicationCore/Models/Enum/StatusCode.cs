using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Models.Enum
{
    public enum StatusCode
    {
        [Description("REQUEST")]
        REQUEST = 1, //예약요청
        [Description("CONFIRMATION")]
        CONFIRMATION = 2, //예약확정
        [Description("CANCELLATION")]
        CANCELLATION = 3, //예약취소
        [Description("CANCELLATION REQUEST")]
        CANCELLATIONREQUEST = 4 // 예약 취소 요청
    }
}
