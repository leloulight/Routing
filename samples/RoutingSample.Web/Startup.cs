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
            var routes = app.BuildRouteTable();

            routes.MapGet("verbs/Get", c => c.Response.WriteAsync("This is a GET"));
            routes.MapPut("verbs/Put", c => c.Response.WriteAsync("This is a PUT"));
            routes.MapPost("verbs/Post", c => c.Response.WriteAsync("This is a POST"));
            routes.MapDelete("verbs/Delete", c => c.Response.WriteAsync("This is a DELETE"));
            routes.MapRoute("verbs/{verb}", c => c.Response.WriteAsync($"This is a {c.Request.Method}"));

            routes.MapRouteToMiddleware("middleware/{tenant}", a =>
            {
                a.New().Use(next => (context) =>
                {
                    context.Request.Headers["TenantId"] = context.GetRouteValue("tenant");
                    return next(context);
                });

                a.Use(next => (context) =>
                {
                    return context.Response.WriteAsync($"Hello {context.Request.Headers["TenantId"]}");
                });
            });
        }
    }
}