using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DisableValidationAttribute : Attribute
    {
    }
}
