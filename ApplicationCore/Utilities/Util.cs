
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;


namespace AGL.Api.ApplicationCore.Utilities
{
    public class Util
    {
        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
        }
        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public string GetFromToDt(DateTime? from, DateTime? to)
        {
            var ret = "";

            ret = (from == null ? "" : from.Value.ToString("yyyy-MM-dd"));

            if(from!=null && to!=null)
                ret = $"{ret} ~ ";

            ret = $"{ret}{(to == null ? "" : to.Value.ToString("yyyy-MM-dd"))}";

            return ret;
        }

        public string ToPrice(object prc)
        {
            if (prc == null) return "";
            if (prc is Int32)
            {
                return ((Int32)prc).ToString("N0");
            }
            else if (prc is Int64)
            {
                return ((Int64)prc).ToString("N0");
            }
            else
            {
                Int64 n = 0;
                try
                {
                    string s = prc.ToString();
                    int pos = s.LastIndexOf('.');
                    if (pos == -1)
                    {
                        s = Regex.Replace(s, "[^-0-9]", "");
                        n = Int64.Parse(s);
                        return n.ToString("N0");
                    }
                    else
                    {
                        var num = Int64.Parse(Regex.Replace(s.Substring(0, pos), "[^-0-9]", ""));
                        var frac = s.Substring(pos + 1);
                        if (string.IsNullOrEmpty(frac))
                        {
                            return num.ToString("N0");
                        }
                        else
                        {
                            return num.ToString("N0") + "." + frac;
                        }
                    }
                }
                catch { }
                return prc.ToString();
            }
        }

        public static string RSubSting(string Text, int TextLenth) {
            string ConvertText;
            if (Text.Length < TextLenth)
            {
                TextLenth = Text.Length;
            }
            ConvertText = Text.Substring(TextLenth, Text.Length - TextLenth);
            return ConvertText;
        }

        public string addHyphen(string tel) {

            var retunStr = string.Empty;
            var t1 = string.Empty;
            var t2 = string.Empty;
            var t3 = string.Empty;

            tel = tel.Replace("-", "");

            if (tel.Length == 8)     //1588-xxxx
            {
                t1 = tel.Substring(0, 4);
                t2 = tel.Substring(4, 4);
                retunStr = t1 + "-" + t2;
            }
            else if (tel.Length == 9)    //02-xxx-xxxx
            {
                t1 = tel.Substring(0, 2);
                t2 = tel.Substring(2, 3);
                t3 = tel.Substring(5, 4);
                retunStr = t1 + "-" + t2 + "-" + t3;
            }
            else if (tel.Length == 10)
            {
                if (tel.Substring(0, 2) == "01")     //휴대전화 01x-xxx-xxxx
                {
                    t1 = tel.Substring(0, 3);
                    t2 = tel.Substring(3, 3);
                    t3 = tel.Substring(6, 4);
                    retunStr = t1 + "-" + t2 + "-" + t3;
                }
                else if (tel.Substring(0, 2) == "02")
                {
                    t1 = tel.Substring(0, 2);
                    t2 = tel.Substring(2, 4);
                    t3 = tel.Substring(6, 4);
                    retunStr = t1 + "-" + t2 + "-" + t3;
                }
                else
                {
                    t1 = tel.Substring(0, 3);
                    t2 = tel.Substring(3, 3);
                    t3 = tel.Substring(6, 4);
                    retunStr = t1 + "-" + t2 + "-" + t3;
                }
            }
            else if (tel.Length == 11)   //xxx-xxxx-xxxx(휴대전화,070)
            {
                t1 = tel.Substring(0, 3);
                t2 = tel.Substring(3, 4);
                t3 = tel.Substring(7, 4);
                retunStr = t1 + "-" + t2 + "-" + t3;
            }

            return retunStr;
        }

        public string GetStrMark(string str, int len, string strMark)
        {
            var rst = string.Empty;


            char[] chars = str.ToCharArray();
            Array.Reverse(chars);
            
            var tmpStr = new string(chars);
            var iCnt = 1;

            foreach(var item in tmpStr)
            {
                

                rst += item.ToString();
                if (iCnt % len == 0 && iCnt!=tmpStr.Length)
                    rst += strMark;

                iCnt++;
            }


            char[] rstChars = rst.ToCharArray();
            Array.Reverse(rstChars);


            return new string(rstChars);


        }


        public string DisplayK(int i)
        {
            string rst = string.Empty;

            if (i >= 1000)
            {
                rst = $"{(i / 1000).ToString("#,##0")}K";

            }
            else
                rst = i.ToString();


            return rst;

        }


        public bool SetIsMedia(string ext)
        {
            return new string[] { "mp4", "mov", "wmv", "webm", "mxf", "avi", "avchd", "hevc" }.Contains(ext.ToLower()) ? true : false;
        }


        // 랜덤 GUID
        public static string GenerateRandomGuid(int length = 20)
        {
            string guid = Guid.NewGuid().ToString("N"); // 32자리 고유 문자열
            return guid.Substring(0, length);
        }

        // 랜덤 문자열
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        // 랜덤 숫자
        public static string GenerateRandomNumber(int length)
        {
            const string numbers = "0123456789";
            return new string(Enumerable.Repeat(numbers, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        // 랜덤 날짜
        public static DateTime GenerateRandomDate()
        {
            var random = new Random();
            return DateTime.Today.AddDays(random.Next(1, 30)); // Random date within the next 30 days
        }

        // 랜덤 시간
        public static TimeSpan GenerateRandomTime()
        {
            var random = new Random();
            return new TimeSpan(random.Next(6, 18), random.Next(0, 60), 0); // Random time between 6:00 and 18:59
        }

        // 랜덤 금액
        public static decimal GenerateRandomAmount()
        {
            var random = new Random();
            return random.Next(100, 1000) + (decimal)random.NextDouble(); // Random amount between 100 and 1000
        }

    }
}
