using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Utilities
{
    // JsonConverter를 상속받아 숫자와 문자열을 처리하는 컨버터를 작성
    public class DecimalOrStringConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 숫자 값 처리
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDecimal(out var number))
                    return number;
            }
            // 문자열 값 처리
            else if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (decimal.TryParse(stringValue, out var result))
                    return result;
            }
            // null 처리
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // 숫자도 문자열도 아니면 예외 발생
            throw new JsonException("Invalid value for decimal.");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
