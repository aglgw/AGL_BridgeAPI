using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EnvironmentSpecificAttribute : Attribute
    {
        public string Environment { get; }

        public EnvironmentSpecificAttribute(string environment)
        {
            Environment = environment;
        }
    }
}
