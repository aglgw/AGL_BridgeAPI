using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SkipAuthenticationAttribute : Attribute
    {
    }
}
