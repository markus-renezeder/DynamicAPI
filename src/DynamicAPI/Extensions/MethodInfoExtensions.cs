using DynamicAPI.Attributes;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicAPI.Extensions
{
    internal static class MethodInfoExtensions
    {

        internal static IEnumerable<HttpMethodAttribute> HttpMethodAttributes(this MethodInfo method)
        {
            var attributes = method.GetCustomAttributes<HttpMethodAttribute>();

            return attributes;
        }

       
    }
}
