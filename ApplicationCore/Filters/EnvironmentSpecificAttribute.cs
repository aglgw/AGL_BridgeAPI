using Microsoft.Extensions.Hosting;
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
        public string[] Environments { get; }

        public EnvironmentSpecificAttribute(params string[] environments)
        {
            Environments = environments;
        }

        public bool IsEnvironmentAllowed(string currentEnvironment)
        {
            return Environments.Contains(currentEnvironment, StringComparer.OrdinalIgnoreCase);
        }
    }
}
