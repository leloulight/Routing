// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing.Internal;

namespace Microsoft.AspNet.Routing
{
    public class Route : RouteBase
    {
        private readonly IRouteEndpoint _target;

        public Route(
            IRouteEndpoint target,
            string routeTemplate,
            IInlineConstraintResolver inlineConstraintResolver)
            : this(
                target,
                routeTemplate,
                defaults: null,
                constraints: null,
                dataTokens: null,
                inlineConstraintResolver: inlineConstraintResolver)
        {
        }

        public Route(
            IRouteEndpoint target,
            string routeTemplate,
            IDictionary<string, object> defaults,
            IDictionary<string, object> constraints,
            IDictionary<string, object> dataTokens,
            IInlineConstraintResolver inlineConstraintResolver)
            : this(target, null, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
        }

        public Route(
            IRouteEndpoint target,
            string routeName,
            string routeTemplate,
            IDictionary<string, object> defaults,
            IDictionary<string, object> constraints,
            IDictionary<string, object> dataTokens,
            IInlineConstraintResolver inlineConstraintResolver)
            : base(routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _target = target;
        }

        public override Task OnRouteMatchedAsync(RouteContext context)
        {
            context.Handler = _target.CreateHandler(context.RouteData);
            return TaskCache.CompletedTask;
        }
    }
}
