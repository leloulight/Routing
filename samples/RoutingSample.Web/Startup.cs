// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace RoutingSample.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            var endpoint1 = new DelegateRouteEndpoint((context, routeData) =>
            {
                return context.Response.WriteAsync("match1, route values -" + routeData.Values.Print());
            });

            var endpoint2 = new DelegateRouteEndpoint((context) => context.Response.WriteAsync("Hello, World!"));

            var routeBuilder = app.UseRouter(endpoint1);

            routeBuilder.AddPrefixRoute("api/store");

            routeBuilder.MapRoute("defaultRoute",
                                  "api/constraint/{controller}",
                                  null,
                                  new { controller = "my.*" });
            routeBuilder.MapRoute("regexStringRoute",
                                  "api/rconstraint/{controller}",
                                  new { foo = "Bar" },
                                  new { controller = new RegexRouteConstraint("^(my.*)$") });
            routeBuilder.MapRoute("regexRoute",
                                  "api/r2constraint/{controller}",
                                  new { foo = "Bar2" },
                                  new
                                  {
                                      controller = new RegexRouteConstraint(
                                          new Regex("^(my.*)$", RegexOptions.None, TimeSpan.FromSeconds(10)))
                                  });

            routeBuilder.MapRoute("parameterConstraintRoute",
                                  "api/{controller}/{*extra}",
                                  new { controller = "Store" });

            routeBuilder.AddPrefixRoute("hello/world", endpoint2);

            routeBuilder.MapLocaleRoute("en-US", "store/US/{action}", new { controller = "Store" });
            routeBuilder.MapLocaleRoute("en-GB", "store/UK/{action}", new { controller = "Store" });

            routeBuilder.AddPrefixRoute("", endpoint2);

            app.UseRouter(routeBuilder.Build());
        }
    }
}