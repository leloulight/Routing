// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Builder
{
    public class RouterMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly IRouter _router;

        public RouterMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IRouter router)
        {
            _next = next;
            _router = router;

            _logger = loggerFactory.CreateLogger<RouterMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(_router);

            await _router.RouteAsync(context);

            if (context.Handler == null)
            {
                _logger.LogVerbose("Request did not match any routes.");

                await _next.Invoke(httpContext);
            }
            else
            {
                var feature = new DefaultRouterFeature()
                {
                    RouteData = context.RouteData,
                    UrlGenerator = (name, values) => GenerateUrl(context, name, values),
                };

                context.HttpContext.Features[typeof(IRouterFeature)] = feature;
                await context.Handler(context.HttpContext, context.RouteData);
            }
        }

        private PathString GenerateUrl(RouteContext context, string name, object values)
        {
            if (context?.RouteData?.Routers == null || context.RouteData.Routers.Count == 0)
            {
                return new PathString();
            }

            var virtualPathContext = new VirtualPathContext(context.HttpContext, context.RouteData.Values, new RouteValueDictionary(values));
            var result = context.RouteData.Routers[0].GetVirtualPath(virtualPathContext);

            return result.VirtualPath;
        }
    }
}
