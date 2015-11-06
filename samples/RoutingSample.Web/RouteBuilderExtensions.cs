// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Routing;

namespace RoutingSample.Web
{
    public static class RouteBuilderExtensions
    {
        public static IRouteBuilder AddPrefixRoute(
            this IRouteBuilder routeBuilder,
            string prefix)
        {
            if (routeBuilder.DefaultHandler == null)
            {
                throw new InvalidOperationException("DefaultHandler must be set.");
            }

            if (routeBuilder.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider must be set.");
            }

            return AddPrefixRoute(routeBuilder, prefix, routeBuilder.DefaultHandler);
        }

        public static IRouteBuilder AddPrefixRoute(
            this IRouteBuilder routeBuilder,
            string prefix,
            IRouteEndpoint handler)
        {
            routeBuilder.Routes.Add(new PrefixRoute(handler, prefix));
            return routeBuilder;
        }

        public static IRouteBuilder MapLocaleRoute(
            this IRouteBuilder routeBuilder,
            string locale,
            string routeTemplate,
            object defaults)
        {
            var defaultsDictionary = new RouteValueDictionary(defaults);
            defaultsDictionary.Add("locale", locale);

            var builder = new RouteSpecBuilder(routeBuilder.ConstraintResolver, routeTemplate)
            {
                Defaults = defaultsDictionary,
            };

            routeBuilder.Routes.Add(new Route(builder.Build(), routeBuilder.DefaultHandler));

            return routeBuilder;
        }
    }
}