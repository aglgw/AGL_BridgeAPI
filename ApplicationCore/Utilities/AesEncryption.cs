using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Utilities
{
    public static class AesEncryption
    {
        // 설정에서 키 값을 로드
        private static byte[] Aes_Key;

        // 설정 파일에서 키 값을 읽는 메소드
        private static byte[] LoadKeyFromConfiguration()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 현재 디렉토리 설정
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // 설정 파일 로드
                .Build();

            return Encoding.UTF8.GetBytes(configuration["Encryption:Aes_Key"]); // "Key"는 appsettings.json에서의 설정 키
        }

        public static byte[] Encrypt(string text)
        {
            Aes_Key = LoadKeyFromConfiguration();

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Aes_Key;
                aesAlg.IV = Aes_Key;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        // AES 복호화
        public static string Decrypt(byte[] cipherText)
        {
            Aes_Key = LoadKeyFromConfiguration();

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Aes_Key;
                aesAlg.IV = Aes_Key;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
