using System.Net.Mail;
using System.Text;
using AGL.Api.ApplicationCore.Models;
using Microsoft.Extensions.Configuration;

namespace AGL.Api.ApplicationCore.Utilities
{
    public static class EmailService
    {
        private static string SendGridHost;
        private static string SendGridPort;
        private static string SendGridApiKey;

        // 설정 파일에서 키 값을 읽는 메소드
        private static void LoadKeyFromConfiguration()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 현재 디렉토리 설정
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // 설정 파일 로드
                .Build();

            SendGridHost = configuration["SendGrid:Host"];
            SendGridPort = configuration["SendGrid:Port"];
            SendGridApiKey = configuration["SendGrid:ApiKey"];

        }

        public static bool SendEmail(EmailInfo emailForm)
        {
            try
            {
                using (var message = CreateMailMessage(emailForm))
                using (var client = CreateSmtpClient())
                {
                    client.Send(message);
                    return true;  // 메일 전송 성공
                }
            }
            catch (Exception ex)
            {
                // 로그 서비스를 사용하여 예외 정보 로깅
                LogService.logInformation($"EmailService > SendEmail Error: Failed due to an exception: {ex}");

                // 콘솔에도 예외 메시지 출력
                Console.WriteLine($"EmailService > SendEmail Error: {ex.Message}");

                return false;  // 메일 전송 실패
            }
        }

        private static MailMessage CreateMailMessage(EmailInfo emailForm)
        {
            var message = new MailMessage
            {
                From = new MailAddress(emailForm.FromEmailAddress, emailForm.FromEmailName),
                Subject = emailForm.Subject,
                Body = emailForm.Body,
                IsBodyHtml = true,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                Priority = MailPriority.Normal,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };
            message.To.Add(emailForm.ToEmailAddress);
            AddAttachments(message, emailForm.AttachFile, emailForm.AttachFileList);

            return message;
        }

        private static void AddAttachments(MailMessage message, string singleAttachment, IList<string> attachmentList)
        {
            if (!string.IsNullOrWhiteSpace(singleAttachment) && File.Exists(singleAttachment))
            {
                message.Attachments.Add(new Attachment(singleAttachment));
            }
            if (attachmentList != null)
            {
                foreach (var filePath in attachmentList.Where(File.Exists))
                {
                    message.Attachments.Add(new Attachment(filePath));
                }
            }
        }

        private static SmtpClient CreateSmtpClient()
        {
            LoadKeyFromConfiguration();

            return new SmtpClient
            {
                Host = SendGridHost,
                Port = Convert.ToInt32(SendGridPort),
                EnableSsl = true,
                Timeout = 4000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential("apikey", SendGridApiKey)
            };
        }
    }
}
