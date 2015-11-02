// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Builder;

namespace Microsoft.AspNet.Routing
{
    public class MiddlewareEndpoint : IRouteEndpoint
    {
        private RoutedRequestDelegate _routeFunc;

        public MiddlewareEndpoint(IApplicationBuilder app)
        {
            App = app;
        }

        protected IApplicationBuilder App { get; }

        protected RoutedRequestDelegate RouteFunc
        {
            get
            {
                if (_routeFunc == null)
                {
                    var appFunc = App.Build();
                    _routeFunc = (httpContext, routeData) => appFunc(httpContext);
                }

                return _routeFunc;
            }
        }

        public RoutedRequestDelegate CreateHandler(RouteData routeData)
        {
            return _routeFunc;
        }
    }
}
