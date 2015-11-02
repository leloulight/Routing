// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Routing.Template;

namespace Microsoft.AspNet.Routing
{
    public class RouteSpecBuilder
    {
        public RouteSpecBuilder(IInlineConstraintResolver constraintResolver, string routeTemplate)
        {
            if (constraintResolver == null)
            {
                throw new ArgumentNullException(nameof(constraintResolver));
            }

            if (routeTemplate == null)
            {
                throw new ArgumentNullException(nameof(routeTemplate));
            }

            ConstraintResolver = constraintResolver;
            RouteTemplate = routeTemplate;
        }

        protected IInlineConstraintResolver ConstraintResolver { get; }

        public IDictionary<string, object> Constraints { get; set; }

        public IDictionary<string, object> DataTokens { get; set; }

        public IDictionary<string, object> Defaults { get; set; }

        public string RouteName { get; set; }

        protected string RouteTemplate { get; }

        public RouteSpec Build()
        {
            var parsedTemplate = TemplateParser.Parse(RouteTemplate);

            var constraintBuilder = new RouteConstraintBuilder(ConstraintResolver, RouteTemplate);
            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.IsOptional)
                {
                    constraintBuilder.SetOptional(parameter.Name);
                }

                foreach (var inlineConstraint in parameter.InlineConstraints)
                {
                    constraintBuilder.AddResolvedConstraint(parameter.Name, inlineConstraint.Constraint);
                }
            }

            if (Constraints != null)
            {
                foreach (var kvp in Constraints)
                {
                    constraintBuilder.AddConstraint(kvp.Key, kvp.Value);
                }
            }

            var constraints = constraintBuilder.Build();
            var dataTokens = new RouteValueDictionary(DataTokens);

            var defaults = Defaults == null ? new RouteValueDictionary() : new RouteValueDictionary(Defaults);

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    if (defaults.ContainsKey(parameter.Name))
                    {
                        throw new InvalidOperationException(
                          Resources.FormatTemplateRoute_CannotHaveDefaultValueSpecifiedInlineAndExplicitly(
                              parameter.Name));
                    }
                    else
                    {
                        defaults.Add(parameter.Name, parameter.DefaultValue);
                    }
                }
            }

            return new RouteSpec(parsedTemplate)
            {
                Constraints = constraints,
                DataTokens = dataTokens,
                Defaults = defaults,
                RouteName = RouteName,
            };
        }
    }
}
