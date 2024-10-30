using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using static AGL.Api.API_Template.Models.OAPI.OAPI;

namespace AGL.Api.API_Template.Models.OAPI
{
    [DataContract]
    public class OAPITeeTimeBaseRequest
    {
        [DataMember]
        public string? golfclubCode { get; set; } // 바디에 있을 수도 있고 없을 수도 있으므로 선택적 사용
    }

    /// <summary>
    /// 골프장 수정
    /// </summary>
    [DataContract]
    public class OAPITeeTimeRequest
    {
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string? golfclubCode { get; set; } // 바디에 있을 수도 있고 없을 수도 있으므로 선택적 사용
        /// <summary>
        /// 날짜적용방법 - 특정 공휴일이 있을시 2번 사용 ( 1 기간 , 2 적용일 )
        /// </summary>
        [DataMember]
        public int dateApplyType { get; set; }
        /// <summary>
        /// 기간 시작일 
        /// </summary>
        [DataMember]
        public string startPlayDate { get; set; }
        /// <summary>
        /// 기간 종료일 
        /// </summary>
        [DataMember]
        public string endPlayDate { get; set; }
        /// <summary>
        /// 예외일 ( 공휴일 적용 ) 
        /// </summary>
        [DataMember]
        public List<string> exceptionDate { get; set; }
        /// <summary>
        /// 기간 중 적용 요일 ( 1 월요일, 2 화요일, 3 수요일, 4 목요일, 5 금요일, 6 토요일, 7 일요일 )
        /// </summary>
        [DataMember]
        public List<int> week { get; set; }
        /// <summary>
        /// 적용일
        /// </summary>
        [DataMember]
        public List<string> effectiveDate { get; set; }
        /// <summary>
        /// 티타임 정보 리스트
        /// </summary>
        [DataMember]
        public List<TeeTimeInfo> TeeTimeInfo { get; set; }
    }

    /// <summary>
    /// 티타임 조회
    /// </summary>
    [DataContract]
    public class OAPITeeTimeGetRequest
    {
        /// <summary>
        /// 시작일
        /// </summary>
        [DataMember]
        public string startDate { get; set; }
        /// <summary>
        /// 종료일
        /// </summary>
        [DataMember]
        public string endDate { get; set; }
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string golfclubCode { get; set; }
    }

    /// <summary>
    /// 티타임 상태수정
    /// </summary>
    [DataContract]
    public class OAPITeeTimetAvailabilityRequest
    {
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string golfclubCode { get; set; }
        /// <summary>
        /// 시작일
        /// </summary>
        [DataMember]
        public string playDate { get; set; }
        /// <summary>
        /// 코스 코드
        /// </summary>
        [DataMember]
        public string courseCode { get; set; }
        /// <summary>
        /// 시간 정보
        /// </summary>
        [DataMember]
        public List<TimeInfo> time { get; set; }
        /// <summary>
        /// 판매여부 ( true  판매, false  판매안함 )
        /// </summary>
        [DataMember]
        public bool available { get; set; }
    }

    /// <summary>
    /// 예약확정
    /// </summary>
    [DataContract]
    public class OAPIReservationRequest
    {
        /// <summary>
        /// 부킹 IDE
        /// </summary>
        [DataMember]
        public string reservationId { get; set; }
    }



    /// <summary>
    ///예약 목록 조회
    /// </summary>
    public class ReqBookingInquiry: OAPITeeTimeBaseRequest
    {
        /// <summary>
        /// 시작일 YYYY-MM-DD
        /// </summary>
        [DataMember, Required]
        public string startDate { get; set; } = string.Empty;

        /// <summary>
        /// 종료일 YYYY-MM-DD
        /// </summary>
        [DataMember, Required]
        public string endtDate { get; set; } = string.Empty;


        /// <summary>
        /// Booking ID
        /// </summary>
        [DataMember]
        public string? reservationId { get; set; }

        /// <summary>
        /// Booking Status
        /// </summary>
        [DataMember]
        public int? status { get; set; }


    }
}
