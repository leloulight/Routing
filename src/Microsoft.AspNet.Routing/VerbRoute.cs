// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Routing.Internal;

namespace Microsoft.AspNet.Routing
{
    public class VerbRoute : Route
    {
        public VerbRoute(RouteSpec routeSpec, IRouteEndpoint target, string verb)
            : base(routeSpec, target)
        {
            Verb = verb;
        }

        public string Verb { get; }

        public override Task OnRouteMatchedAsync(RouteContext context)
        {
            if (context.HttpContext.Request.Method == Verb)
            {
                return base.OnRouteMatchedAsync(context);
            }

            return TaskCache.CompletedTask;
        }
    }
}
