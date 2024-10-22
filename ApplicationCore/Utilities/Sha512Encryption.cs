using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AGL.Api.ApplicationCore.Utilities
{
    public static class Sha512Hashing
    {
        private static string Sha512_Key;
        
        // 설정 파일에서 키 값을 읽는 메소드
        private static string LoadKeyFromConfiguration()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 현재 디렉토리 설정
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // 설정 파일 로드
                .Build();

            return configuration["Encryption:Sha512_Key"]; // "Sha512_Key"는 appsettings.json에서의 설정 키
        }

        // SHA512 해시 함수
        public static string ComputeSha512Hash(string text)
        {
            Sha512_Key = LoadKeyFromConfiguration();

            // 입력 문자열과 키를 결합
            string dataToHash = text + Sha512_Key;

            using (SHA512 sha512Hash = SHA512.Create())
            {
                // 결합된 문자열을 바이트 배열로 인코딩
                byte[] sourceBytes = Encoding.UTF8.GetBytes(dataToHash);

                // 해시 계산
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);

                // 바이트 배열을 HEX 문자열로 변환
                StringBuilder hash = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hash.Append(hashBytes[i].ToString("x2"));
                }

                return hash.ToString();
            }
        }
    }
}
