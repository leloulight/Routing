// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Builder
{
    public static class RouteBuilderExtensions
    {
        public static IRouteBuilder MapRoute(this IRouteBuilder routeCollectionBuilder,
                                             string name,
                                             string template)
        {
            MapRoute(routeCollectionBuilder, name, template, defaults: null);
            return routeCollectionBuilder;
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeCollectionBuilder,
                                             string name,
                                             string template,
                                             object defaults)
        {
            return MapRoute(routeCollectionBuilder, name, template, defaults, constraints: null);
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeCollectionBuilder,
                                             string name,
                                             string template,
                                             object defaults,
                                             object constraints)
        {
            return MapRoute(routeCollectionBuilder, name, template, defaults, constraints, dataTokens: null);
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeCollectionBuilder,
                                             string name,
                                             string template,
                                             object defaults,
                                             object constraints,
                                             object dataTokens)
        {
            if (routeCollectionBuilder.DefaultHandler == null)
            {
                throw new InvalidOperationException(Resources.DefaultHandler_MustBeSet);
            }

            var inlineConstraintResolver = routeCollectionBuilder
                                                        .ServiceProvider
                                                        .GetRequiredService<IInlineConstraintResolver>();
            routeCollectionBuilder.Routes.Add(new TemplateRoute(routeCollectionBuilder.DefaultHandler,
                                                                name,
                                                                template,
                                                                ObjectToDictionary(defaults),
                                                                ObjectToDictionary(constraints),
                                                                ObjectToDictionary(dataTokens),
                                                                inlineConstraintResolver));

            return routeCollectionBuilder;
        }

        public static IRouteBuilder MapRoute(
            this IRouteBuilder builder,
            string template,
            RequestDelegate action)
        {
            return MapRoute(builder, template, (app) => app.Use(next => action));
        }

        public static IRouteBuilder MapRoute(
            this IRouteBuilder builder,
            string template,
            Func<RequestDelegate, RequestDelegate> action)
        {
            return MapRoute(builder, template, (app) => app.Use(action));
        }

        public static IRouteBuilder MapRoute(
            this IRouteBuilder builder,
            string template,
            Action<IApplicationBuilder> setup)
        {
            var pipeline = builder.ApplicationBuilder.New();

            if (setup != null)
            {
                setup(pipeline);
            }

            var endpoint = builder.DefaultHandler;
            pipeline.Use(next => new RouterAdapter(endpoint).Invoke);

            var constraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var route = new TemplateRoute(new MiddlewareAdapter(endpoint, pipeline.Build()), template, null, null, null, constraintResolver);
            builder.Routes.Add(route);

            return builder;
        }

        private static IDictionary<string, object> ObjectToDictionary(object value)
        {
            var dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            return new RouteValueDictionary(value);
        }

        private class MiddlewareAdapter : IRouter
        {
            private readonly IRouter _endpoint;
            private readonly RequestDelegate _middleware;

            public MiddlewareAdapter(IRouter endpoint, RequestDelegate middleware)
            {
                _endpoint = endpoint;
                _middleware = middleware;
            }

            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                return _endpoint.GetVirtualPath(context);
            }

            public async Task RouteAsync(RouteContext context)
            {
                context.HttpContext.SetFeature(typeof(IRoutingFeature), new RoutingFeature() { RouteContext = context });

                context.IsHandled = true;
                await _middleware(context.HttpContext);
            }
        }

        private class RouterAdapter
        {
            private IRouter _endpoint;

            public RouterAdapter(IRouter endpoint)
            {
                _endpoint = endpoint;
            }

            public async Task Invoke(HttpContext httpContext)
            {
                var routeContext = httpContext.GetFeature<IRoutingFeature>()?.RouteContext;
                await _endpoint.RouteAsync(routeContext);
            }
        }

        private class RoutingFeature : IRoutingFeature
        {
            public RouteContext RouteContext { get; set; }
        }
    }
}
