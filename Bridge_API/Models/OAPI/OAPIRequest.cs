﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;

namespace AGL.Api.Bridge_API.Models.OAPI
{
    [DataContract]
    public class TeeTimeBaseRequest
    {
        [DataMember]
        public string? GolfclubCode { get; set; } // 바디에 있을 수도 있고 없을 수도 있으므로 선택적 사용
    }

    /// <summary>
    /// 티타임 등록/수정 요청
    /// </summary>
    [DataContract]
    public class TeeTimeRequest
    {
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string? GolfclubCode { get; set; } // 바디에 있을 수도 있고 없을 수도 있으므로 선택적 사용
        /// <summary>
        /// 날짜적용방법 - 특정 공휴일이 있을시 2번 사용 ( 1 기간 , 2 적용일 )
        /// </summary>
        [DataMember]
        public int DateApplyType { get; set; }
        /// <summary>
        /// 기간 시작일 
        /// </summary>
        [DataMember]
        public string? StartPlayDate { get; set; }
        /// <summary>
        /// 기간 종료일 
        /// </summary>
        [DataMember]
        public string? EndPlayDate { get; set; }
        /// <summary>
        /// 예외일 ( 공휴일 적용 ) 
        /// </summary>
        [DataMember]
        public List<string>? ExceptionDate { get; set; }
        /// <summary>
        /// 기간 중 적용 요일 ( 1 월요일, 2 화요일, 3 수요일, 4 목요일, 5 금요일, 6 토요일, 7 일요일 )
        /// </summary>
        [DataMember]
        public List<int>? Week { get; set; }
        /// <summary>
        /// 적용일
        /// </summary>
        [DataMember]
        public List<string>? EffectiveDate { get; set; }
        /// <summary>
        /// 티타임 정보 리스트
        /// </summary>
        [DataMember]
        public List<TeeTimeInfo> TeeTimeInfo { get; set; }
    }

    /// <summary>
    /// 티타임 등록/수정 백그라운드
    /// </summary>
    [DataContract]
    public class TeeTimeBackgroundRequest : TeeTimeRequest
    {
        /// <summary>
        /// 공급사 코드 
        /// </summary>
        [DataMember]
        public string SupplierCode { get; set; }
    }
        


    /// <summary>
    /// 티타임 조회
    /// </summary>
    [DataContract]
    public class TeeTimeGetRequest
    {
        /// <summary>
        /// 시작일
        /// </summary>
        [DataMember]
        public string StartDate { get; set; }
        /// <summary>
        /// 종료일
        /// </summary>
        [DataMember]
        public string EndDate { get; set; }
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string GolfclubCode { get; set; }
    }

    /// <summary>
    /// 티타임 상태수정
    /// </summary>
    [DataContract]
    public class TeeTimetAvailabilityRequest
    {
        /// <summary>
        /// 골프장 코드
        /// </summary>
        [DataMember]
        public string GolfclubCode { get; set; }
        /// <summary>
        /// 시작일
        /// </summary>
        [DataMember]
        public string PlayDate { get; set; }
        /// <summary>
        /// 코스 코드
        /// </summary>
        [DataMember]
        public List<string> CourseCode { get; set; }
        /// <summary>
        /// 시간 정보
        /// </summary>
        [DataMember]
        public List<TimeInfo> Time { get; set; }
        /// <summary>
        /// 판매여부 ( true  판매, false  판매안함 )
        /// </summary>
        [DataMember]
        public bool Available { get; set; }
    }

    /// <summary>
    /// 예약확정
    /// </summary>
    [DataContract]
    public class ReservationRequest
    {
        /// <summary>
        /// 부킹 IDE
        /// </summary>
        [DataMember]
        public string ReservationId { get; set; }
    }



    /// <summary>
    ///예약 목록 조회
    /// </summary>
    public class ReqBookingInquiry: TeeTimeBaseRequest
    {
        /// <summary>
        /// 시작일 YYYY-MM-DD
        /// </summary>
        [DataMember, Required]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// 종료일 YYYY-MM-DD
        /// </summary>
        [DataMember, Required]
        public string EndtDate { get; set; } = string.Empty;


        /// <summary>
        /// Booking ID
        /// </summary>
        [DataMember]
        public string? ReservationId { get; set; }

        /// <summary>
        /// Booking Status
        /// </summary>
        [DataMember]
        public int? Status { get; set; }


    }

    /// <summary>
    ///예약 요청
    /// </summary>
    public class ReqBookingRequest : TeeTimeBaseRequest
    {
        /// <summary>
        /// 코스 코드
        /// </summary>
        [DataMember]
        public string CourseCode { get; set; }
        /// <summary>
        /// 예약요청일 YYYY-MM-DD
        /// </summary>
        [DataMember]
        public string ReservationDate { get; set; }
        /// <summary>
        /// 시작시간 HHMM
        /// </summary>
        [DataMember]
        public string ReservationStartTime { get; set; }
        /// <summary>
        /// 플레이어 수
        /// </summary>
        [DataMember]
        public int ReservationMembers { get; set; }
        /// <summary>
        /// 화폐
        /// </summary>
        [DataMember]
        public string Currency { get; set; }
        /// <summary>
        /// 총요금
        /// </summary>
        [DataMember]
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// 예약자명
        /// </summary>
        [DataMember]
        public string HolderName { get; set; }
        /// <summary>
        /// 예약자 연락처
        /// </summary>
        [DataMember]
        public string ReservationPhone { get; set; }
        /// <summary>
        /// 예약자 이메일
        /// </summary>
        [DataMember]
        public string ReservationEmail { get; set; }
        /// <summary>
        /// 예약자 국적
        /// </summary>
        [DataMember]
        public string ReservationCountry { get; set; }
        /// <summary>
        /// 내장객 정보
        /// </summary>
        [DataMember]
        public List<GuestInfo>? GuestInfo { get; set; }
        
    }

    /// <summary>
    ///예약 요청
    /// </summary>
    public class GuestInfo
    {
        /// <summary>
        /// 내장객명
        /// </summary>
        [DataMember]
        public string? GuestName { get; set; }
        /// <summary>
        /// 내장객 연락처
        /// </summary>
        [DataMember]
        public string? GuestPhone { get; set; }
        /// <summary>
        /// 내장객 성별 F: 여성 M: 남성
        /// </summary>
        [DataMember]
        public string? GuestGender { get; set; }
        /// <summary>
        /// 내장객 국적
        /// </summary>
        [DataMember]
        public string? GuestCountry { get; set; }
    }


}
