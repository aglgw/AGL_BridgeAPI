using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace AGL.Api.ApplicationCore.Extensions
{
    public static class ExtensionMethods
    {
        public static T CloneObject<T>(this T self)
        {
            T newObj = Activator.CreateInstance<T>();

            foreach (PropertyInfo i in newObj.GetType().GetProperties())
            {
                //"EntitySet" is specific to link and this conditional logic is optional/can be ignored
                if (i.CanWrite && i.PropertyType.Name.Contains("EntitySet") == false)
                {
                    if(!i.GetCustomAttributes(false).Any(x => x.GetType().Name == "ForeignKeyAttribute" || x.GetType().Name == "InversePropertyAttribute"))
                    {
                        object value = self.GetType().GetProperty(i.Name).GetValue(self, null);
                        i.SetValue(newObj, value, null);
                    }
                }
            }

            return newObj;
        }

        public static string GetDescription<T>(T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        public static string Description<T>(this T enumValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }
    }
}
