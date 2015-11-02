// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Builder;

namespace Microsoft.AspNet.Routing
{
    public class DelegateRouteEndpoint : IRouteEndpoint
    {
        public delegate Task RoutedDelegate(RouteContext context);

        private readonly RoutedRequestDelegate _appFunc;

        public DelegateRouteEndpoint(RequestDelegate appFunc)
        {
            _appFunc = (httpContext, routeData) => appFunc(httpContext);
        }

        public DelegateRouteEndpoint(RoutedRequestDelegate appFunc)
        {
            _appFunc = appFunc;
        }

        public RoutedRequestDelegate CreateHandler(RouteData routeData)
        {
            return _appFunc;
        }
    }
}
