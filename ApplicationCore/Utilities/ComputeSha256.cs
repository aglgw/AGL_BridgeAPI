using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Utilities
{
    public class ComputeSha256
    {

        // SHA-256 해싱 메서드
        public static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static string ComputeSha256RequestHash<T>(T request)
        {
            using (var sha256 = SHA256.Create())
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var bytes = Encoding.UTF8.GetBytes(json);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
