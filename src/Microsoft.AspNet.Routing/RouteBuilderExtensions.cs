// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add routes.
    /// </summary>
    public static class RouteBuilderExtensions
    {
        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> with the specified name and template.
        /// </summary>
        /// <param name="routeBuilder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder,
                                             string name,
                                             string template)
        {
            MapRoute(routeBuilder, name, template, defaults: null);
            return routeBuilder;
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> with the specified name, template, and default values.
        /// </summary>
        /// <param name="routeBuilder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">An object that contains default values for route parameters. The object's properties represent the names and values of the default values.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder,
                                             string name,
                                             string template,
                                             object defaults)
        {
            return MapRoute(routeBuilder, name, template, defaults, constraints: null);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> with the specified name, template, default values, and constraints.
        /// </summary>
        /// <param name="routeBuilder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">An object that contains default values for route parameters. The object's properties represent the names and values of the default values.</param>
        /// <param name="constraints">An object that contains constraints for the route. The object's properties represent the names and values of the constraints.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder,
                                             string name,
                                             string template,
                                             object defaults,
                                             object constraints)
        {
            return MapRoute(routeBuilder, name, template, defaults, constraints, dataTokens: null);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> with the specified name, template, default values, and data tokens.
        /// </summary>
        /// <param name="routeBuilder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">An object that contains default values for route parameters. The object's properties represent the names and values of the default values.</param>
        /// <param name="constraints">An object that contains constraints for the route. The object's properties represent the names and values of the constraints.</param>
        /// <param name="dataTokens">An object that contains data tokens for the route. The object's properties represent the names and values of the data tokens.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapRoute(
            this IRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints,
            object dataTokens)
        {
            if (routeBuilder.DefaultHandler == null)
            {
                throw new InvalidOperationException(Resources.DefaultHandler_MustBeSet);
            }

            var builder = new RouteSpecBuilder(routeBuilder.ConstraintResolver, template)
            {
                Constraints = ObjectToDictionary(constraints),
                DataTokens = ObjectToDictionary(dataTokens),
                Defaults = ObjectToDictionary(defaults),
                RouteName = name,
            };

            routeBuilder.Routes.Add(new Route(builder.Build(), routeBuilder.DefaultHandler));

            return routeBuilder;
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder, string template, RequestDelegate handler)
        {
            return routeBuilder.MapRoute(template, new DelegateRouteEndpoint(handler));
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder, string template, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapRoute(template, new DelegateRouteEndpoint(handler));
        }

        public static IRouteBuilder MapRouteToMiddleware(this IRouteBuilder routeBuilder, string template, IApplicationBuilder handler)
        {
            return routeBuilder.MapRoute(template, new MiddlewareEndpoint(handler));
        }

        public static IRouteBuilder MapRouteToMiddleware(this IRouteBuilder routeBuilder, string template, Action<IApplicationBuilder> handler)
        {
            var builder = routeBuilder.ApplicationBuilder.New();
            handler(builder);
            return routeBuilder.MapRoute(template, new MiddlewareEndpoint(builder));
        }

        public static IRouteBuilder MapRoute(this IRouteBuilder routeBuilder, string template, IRouteEndpoint handler)
        {
            var builder = new RouteSpecBuilder(routeBuilder.ConstraintResolver, template);
            routeBuilder.Routes.Add(new Route(builder.Build(), handler));
            return routeBuilder;
        }

        public static IRouteBuilder MapDelete(this IRouteBuilder routeBuilder, string template, RequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "DELETE", handler);
        }

        public static IRouteBuilder MapDelete(this IRouteBuilder routeBuilder, string template, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "DELETE", handler);
        }

        public static IRouteBuilder MapGet(this IRouteBuilder routeBuilder, string template, RequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "GET", handler);
        }

        public static IRouteBuilder MapGet(this IRouteBuilder routeBuilder, string template, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "GET", handler);
        }

        public static IRouteBuilder MapPost(this IRouteBuilder routeBuilder, string template, RequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "POST", handler);
        }

        public static IRouteBuilder MapPost(this IRouteBuilder routeBuilder, string template, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "POST", handler);
        }

        public static IRouteBuilder MapPut(this IRouteBuilder routeBuilder, string template, RequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "PUT", handler);
        }

        public static IRouteBuilder MapPut(this IRouteBuilder routeBuilder, string template, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, "PUT", handler);
        }

        public static IRouteBuilder MapVerb(this IRouteBuilder routeBuilder, string template, string verb, RequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, verb, new DelegateRouteEndpoint(handler));
        }

        public static IRouteBuilder MapVerb(this IRouteBuilder routeBuilder, string template, string verb, RoutedRequestDelegate handler)
        {
            return routeBuilder.MapVerb(template, verb, new DelegateRouteEndpoint(handler));
        }

        public static IRouteBuilder MapVerb(this IRouteBuilder routeBuilder, string template, string verb, IRouteEndpoint handler)
        {
            var builder = new RouteSpecBuilder(routeBuilder.ConstraintResolver, template);
            routeBuilder.Routes.Add(new VerbRoute(builder.Build(), handler, verb));
            return routeBuilder;
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
    }
}
