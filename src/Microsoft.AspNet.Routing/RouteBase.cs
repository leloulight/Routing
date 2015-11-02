// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Routing
{
    public abstract class RouteBase : IRouter
    {
        private readonly RouteSpec _routeSpec;
        private readonly TemplateMatcher _matcher;
        private readonly TemplateBinder _binder;

        private ILogger _logger;
        private ILogger _constraintLogger;

        public RouteBase(RouteSpec routeSpec)
        {
            _routeSpec = routeSpec;

            _matcher = new TemplateMatcher(routeSpec.RouteTemplate, routeSpec.Defaults);
            _binder = new TemplateBinder(routeSpec.RouteTemplate, routeSpec.Defaults);
        }

        public IReadOnlyDictionary<string, IRouteConstraint> Constraints => _routeSpec.Constraints;

        public IReadOnlyDictionary<string, object> DataTokens => _routeSpec.DataTokens;

        public IReadOnlyDictionary<string, object> Defaults => _routeSpec.Defaults;

        public string Name => _routeSpec.RouteName;

        public RouteTemplate ParsedTemplate => _routeSpec.RouteTemplate;

        public string RouteTemplate => _routeSpec.RouteTemplate.OriginalText;

        public abstract Task OnRouteMatchedAsync(RouteContext context);

        public virtual async Task RouteAsync(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            EnsureLoggers(context.HttpContext);

            var requestPath = context.HttpContext.Request.Path;
            var values = _matcher.Match(requestPath);

            if (values == null)
            {
                // If we got back a null value set, that means the URI did not match
                return;
            }

            var oldRouteData = context.RouteData;

            var newRouteData = new RouteData(oldRouteData);

            // Perf: Avoid accessing data tokens if you don't need to write to it, these dictionaries are all
            // created lazily.
            if (_routeSpec.DataTokens.Count > 0)
            {
                MergeValues(newRouteData.DataTokens, _routeSpec.DataTokens);
            }

            MergeValues(newRouteData.Values, values);

            if (!RouteConstraintMatcher.Match(
                Constraints,
                newRouteData.Values,
                context.HttpContext,
                this,
                RouteDirection.IncomingRequest,
                _constraintLogger))
            {
                return;
            }

            _logger.LogVerbose(
                "Request successfully matched the route with name '{RouteName}' and template '{RouteTemplate}'.",
                Name,
                RouteTemplate);

            try
            {
                context.RouteData = newRouteData;

                await OnRouteMatchedAsync(context);
            }
            finally
            {
                // Restore the original values to prevent polluting the route data.
                if (context.Handler == null)
                {
                    context.RouteData = oldRouteData;
                }
            }
        }

        public virtual VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var values = _binder.GetValues(context.AmbientValues, context.Values);
            if (values == null)
            {
                // We're missing one of the required values for this route.
                return null;
            }

            EnsureLoggers(context.Context);
            if (!RouteConstraintMatcher.Match(
                Constraints,
                values.CombinedValues,
                context.Context,
                this,
                RouteDirection.UrlGeneration,
                _constraintLogger))
            {
                return null;
            }

            // When we still cannot produce a value, this should return null.
            var tempPath = _binder.BindValues(values.AcceptedValues);
            if (tempPath == null)
            {
                return null;
            }

            var pathData = new VirtualPathData(this, tempPath);
            if (DataTokens != null)
            {
                foreach (var dataToken in DataTokens)
                {
                    pathData.DataTokens.Add(dataToken.Key, dataToken.Value);
                }
            }

            return pathData;
        }

        private static void MergeValues(
            IDictionary<string, object> destination,
            RouteValueDictionary values)
        {
            foreach (var kvp in values)
            {
                // This will replace the original value for the specified key.
                // Values from the matched route will take preference over previous
                // data in the route context.
                destination[kvp.Key] = kvp.Value;
            }
        }

        private void EnsureLoggers(HttpContext context)
        {
            if (_logger == null)
            {
                var factory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                _logger = factory.CreateLogger<RouteBase>();
                _constraintLogger = factory.CreateLogger(typeof(RouteConstraintMatcher).FullName);
            }
        }

        public override string ToString()
        {
            return RouteTemplate;
        }
    }
}
