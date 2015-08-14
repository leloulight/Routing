// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Routing;
using Microsoft.Framework.Internal;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRouter([NotNull] this IApplicationBuilder builder, [NotNull] IRouter router)
        {
            return builder.UseMiddleware<RouterMiddleware>(router);
        }

        public static IApplicationBuilder UseRouter([NotNull] this IApplicationBuilder builder, Action<IRouteBuilder> a)
        {
            var routeBuilder = new RouteBuilder()
            {
                ApplicationBuilder = builder,
                DefaultHandler = new PassThroughEndpoint(),
                ServiceProvider = builder.ApplicationServices,
            };

            if (a != null)
            {
                a(routeBuilder);
            }

            var router = routeBuilder.Build();
            return UseRouter(builder, router);
        }

        private class PassThroughEndpoint : IRouter
        {
            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                context.IsBound = true;
                return null;
            }

            public Task RouteAsync(RouteContext context)
            {
                context.IsHandled = true;
                return Task.FromResult(0);
            }
        }
    }
}