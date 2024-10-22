using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGL.Api.ApplicationCore.Extensions
{
    public static class GenericExtension
    {
        public static string GetGenericTypeName(this Type type)
        {
            var typeName = string.Empty;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }

        public static string ConvertDictionaryToString<DKey, DValue>(this Dictionary<DKey, DValue> dict)
        {
            string format = "{0}='{1}',";

            StringBuilder itemString = new StringBuilder();
            foreach (KeyValuePair<DKey, DValue> kv in dict)
            {
                itemString.AppendFormat(format, kv.Key, kv.Value);
            }
            itemString.Remove(itemString.Length - 1, 1);

            return itemString.ToString();
        }
    }
}
