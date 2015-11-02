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

            var routes = app.UseRouter(endpoint1);

            routes.MapGet("verbs/Get", c => c.Response.WriteAsync("This is a GET"));
            routes.MapPut("verbs/Put", c => c.Response.WriteAsync("This is a PUT"));
            routes.MapPost("verbs/Post", c => c.Response.WriteAsync("This is a POST"));
            routes.MapDelete("verbs/Delete", c => c.Response.WriteAsync("This is a DELETE"));
            routes.MapRoute("verbs/{verb}", c => c.Response.WriteAsync($"This is a {c.Request.Method}"));

            routes.AddPrefixRoute("api/store");

            routes.MapRoute("defaultRoute",
                                  "api/constraint/{controller}",
                                  null,
                                  new { controller = "my.*" });
            routes.MapRoute("regexStringRoute",
                                  "api/rconstraint/{controller}",
                                  new { foo = "Bar" },
                                  new { controller = new RegexRouteConstraint("^(my.*)$") });
            routes.MapRoute("regexRoute",
                                  "api/r2constraint/{controller}",
                                  new { foo = "Bar2" },
                                  new
                                  {
                                      controller = new RegexRouteConstraint(
                                          new Regex("^(my.*)$", RegexOptions.None, TimeSpan.FromSeconds(10)))
                                  });

            routes.MapRoute("parameterConstraintRoute",
                                  "api/{controller}/{*extra}",
                                  new { controller = "Store" });

            routes.AddPrefixRoute("hello/world", endpoint2);

            routes.MapLocaleRoute("en-US", "store/US/{action}", new { controller = "Store" });
            routes.MapLocaleRoute("en-GB", "store/UK/{action}", new { controller = "Store" });

            routes.AddPrefixRoute("", endpoint2);

            app.UseRouter(routes.Build());
        }
    }
}