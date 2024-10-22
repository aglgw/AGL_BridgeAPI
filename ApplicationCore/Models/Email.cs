namespace AGL.Api.ApplicationCore.Models
{
    public class EmailInfo
    {
        // 필수 메일 정보
        public string FromEmailAddress { get; set; }
        public string FromEmailName { get; set; }
        public string ToEmailAddress { get; set; }
        public string Subject { get; set; } = string.Empty; // 기본값 설정
        public string Body { get; set; } = string.Empty; // 기본값 설정

        // 옵션: null 허용으로 명시
        public string? SignatureFile { get; set; }
        public string? Bcc { get; set; }

        // 첨부파일
        public string? AttachFile { get; set; }
        public List<string>? AttachFileList { get; set; } = new List<string>(); // 빈 리스트로 초기화
       
    }
}
