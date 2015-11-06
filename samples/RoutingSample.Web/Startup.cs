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
                a.Use(next => (context) =>
                {
                    context.Request.Headers["TenantId"] = context.GetRouteValue("tenant");
                    return next(context);
                });

                a.Use(next => (context) =>
                {
                    return context.Response.WriteAsync($"Hello {context.Request.Headers["TenantId"]}");
                });
            });


            routes.Map<ProductHandler>("api/products", (m) =>
            {
                m.MapGet("", c => c.GetProducts);
                m.MapGet("{id}", c => c.GetProductById);
                m.MapPost("{id}", c => c.UpdateProduct);
            });
        }
    }

    public class ProductHandler
    {
        public void GetProducts(HttpContext context)
        {
            var id = context.GetRouteValue("id");
        }

        public void GetProductById(HttpContext context)
        {
        }

        public void UpdateProduct(HttpContext context)
        {
        }
    }

    public static class RouteBuilderExtensions2
    {
        public static Mapper<T> Map<T>(this IRouteBuilder builder)
        {
            return new Mapper<T>(builder);
        }

        public static Mapper<T> Map<T>(this IRouteBuilder builder, string prefix)
        {
            return new Mapper<T>(builder);
        }

        public static Mapper<T> Map<T>(this IRouteBuilder builder, Action<Mapper<T>> mappings)
        {
            var mapper = builder.Map<T>();
            mappings(mapper);
            return mapper;
        }

        public static Mapper<T> Map<T>(this IRouteBuilder builder, string prefix, Action<Mapper<T>> mappings)
        {
            var mapper = builder.Map<T>(prefix);
            mappings(mapper);
            return mapper;
        }
    }

    public class Mapper<T>
    {
        public Mapper(IRouteBuilder builder)
        {
            Builder = builder;
        }

        public IRouteBuilder Builder { get; }

        public Mapper<T> MapGet(string template, Func<T, Action<HttpContext>> action)
        {
            return this;
        }

        public Mapper<T> MapPut(string template, Func<T, Action<HttpContext>> action)
        {
            return this;
        }

        public Mapper<T> MapPost(string template, Func<T, Action<HttpContext>> action)
        {
            return this;
        }

        public Mapper<T> MapDelete(string template, Func<T, Action<HttpContext>> action)
        {
            return this;
        }

        public Mapper<T> MapVerb(string template, string verb, Func<T, Action<HttpContext>> action)
        {
            return this;
        }

        public Mapper<T> MapRoute(string template, Func<T, Action<HttpContext>> action)
        {
            return this;
        }
    }
}