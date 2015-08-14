using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Routing
{
    public static class HttpContextExtensions
    {
        public static IDictionary<string, object> GetRouteValues(this HttpContext context)
        {
            return context.GetFeature<IRoutingFeature>()?.RouteContext?.RouteData?.Values;
        }
    }
}
