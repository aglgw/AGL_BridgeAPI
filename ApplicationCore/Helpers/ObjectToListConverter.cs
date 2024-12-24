using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Helpers
{
    public class ObjectToListConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // 단일 문자열을 리스트로 변환
                var singleValue = reader.GetString();
                return new List<string> { singleValue! };
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // JSON 배열을 읽음
                return JsonSerializer.Deserialize<List<string>>(ref reader, options) ?? new List<string>();
            }
            throw new JsonException("Invalid JSON format for CourseCode. Expected a string or an array of strings.");
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            if (value.Count == 1)
            {
                // 단일 값이면 문자열로 직렬화
                writer.WriteStringValue(value[0]);
            }
            else
            {
                // 여러 값이면 배열로 직렬화
                JsonSerializer.Serialize(writer, value, options);
            }
        }

        // Helper method for manual conversion
        public static List<string> ConvertToListOfStrings(object? value, string fieldName)
        {
            if (value == null) // null 처리
            {
                return new List<string>(); // 빈 리스트 반환
            }

            if (value is string singleValue)
            {
                return new List<string> { singleValue };
            }
            else if (value is IEnumerable<object> enumerable)
            {
                try
                {
                    return enumerable.Select(item => item?.ToString() ?? throw new InvalidOperationException($"Invalid item in field '{fieldName}'. Elements must be strings.")).ToList();
                    //return enumerable.Cast<string>().ToList();
                }
                catch
                {
                    throw new InvalidOperationException($"Invalid data in field '{fieldName}'. All elements must be strings.");
                }
            }
            throw new InvalidOperationException($"Invalid format for field '{fieldName}'. Expected a string or an array of strings.");
        }
    }
}
