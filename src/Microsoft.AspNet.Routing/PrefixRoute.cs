// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing.Internal;

namespace Microsoft.AspNet.Routing
{
    public class PrefixRoute : RouteBase
    {
        private readonly IRouteEndpoint _targetEndpoint;
        private readonly IRouter _targetRouter;

        public PrefixRoute(RouteSpec routeSpec, IRouter target)
            : base(routeSpec)
        {
            _targetRouter = target;
        }

        public PrefixRoute(RouteSpec routeSpec, IRouteEndpoint target)
            : base(routeSpec)
        {
            _targetEndpoint = target;
        }

        public override Task OnRouteMatchedAsync(RouteContext context)
        {
            var request = context.HttpContext.Request;

            var builder = new StringBuilder(request.PathBase);
            var enumerator = new PathTokenizer(request.Path).GetEnumerator();
            for (var i = 0; i < ParsedTemplate.Segments.Count && enumerator.MoveNext(); i++)
            {
                builder.Append('/');
                builder.Append(enumerator.Current.ToString());
            }

            request.PathBase += builder.ToString();
            request.Path = request.Path.Value.Substring(enumerator.Current.Offset + enumerator.Current.Offset);

            if (_targetRouter == null)
            {
                context.Handler = _targetEndpoint.CreateHandler(context.RouteData);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _targetRouter.RouteAsync(context);
            }
        }
    }
}
