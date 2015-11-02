// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Routing.Template;

namespace Microsoft.AspNet.Routing
{
    public class RouteSpec
    {
        public RouteSpec(RouteTemplate routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }

        public IReadOnlyDictionary<string, IRouteConstraint> Constraints { get; set; }

        public RouteValueDictionary DataTokens { get; set; }

        public RouteValueDictionary Defaults { get; set; }

        public string RouteName { get; set; }

        public RouteTemplate RouteTemplate { get; }
    }
}
