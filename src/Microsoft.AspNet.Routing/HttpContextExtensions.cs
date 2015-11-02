// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

namespace Microsoft.AspNet.Routing
{
    public static class HttpContextExtensions
    {
        public static string GetRouteValue(this HttpContext httpContext, string key)
        {
            return httpContext.Features.Get<IRouterFeature>()?.RouteData?.Values[key] as string;
        }
    }
}
