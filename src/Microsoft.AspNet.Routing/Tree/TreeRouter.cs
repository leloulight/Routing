// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Routing.Internal;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Routing.Tree
{
    public class TreeRouter : IRouter
    {
        private readonly UrlMatchingTree[] _trees;

        // Left as an exercise to the reader.
        private readonly ILogger _logger;
        private readonly ILogger _constraintLogger;

        public TreeRouter(UrlMatchingTree[] trees)
        {
            _trees = trees;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            // Left as an exercise to the reader.
            throw new NotImplementedException();
        }

        public async Task RouteAsync(RouteContext context)
        {
            var match = default(TemplateMatch);
            foreach (var tree in _trees)
            {
                var tokenizer = new PathTokenizer(context.HttpContext.Request.Path);
                var enumerator = tokenizer.GetEnumerator();
                var current = tree.Root;

                if ((match = Match(context, current, enumerator)) != default(TemplateMatch))
                {
                    break;
                }
            }

            if (match == default(TemplateMatch))
            {
                return;
            }

            var oldRouteData = context.RouteData;

            var newRouteData = new RouteData(oldRouteData);


            newRouteData.Routers.Add(match.Entry.Target);
            MergeValues(newRouteData.Values, match.Values);

            if (!RouteConstraintMatcher.Match(
                match.Entry.Constraints,
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
                match.Entry.Name,
                match.Entry.RouteTemplate);

            try
            {
                context.RouteData = newRouteData;

                await match.Entry.Target.RouteAsync(context);
            }
            finally
            {
                // Restore the original values to prevent polluting the route data.
                if (!context.IsHandled)
                {
                    context.RouteData = oldRouteData;
                }
            }
        }

        private TemplateMatch Match(RouteContext context, UrlMatchingNode current, PathTokenizer.Enumerator enumerator)
        {
            if (!enumerator.MoveNext())
            {
                // We've reached the end of the Path. Check the matches.
                foreach (var match in current.Matches)
                {
                    // We may want to build something more efficient than TemplateMatcher.
                    // We already test all the literals, and that the shape matches, and that doesn't
                    // need to be redone.
                    var values = match.TemplateMatcher.Match(context.HttpContext.Request.Path);
                    if (values != null)
                    {
                        return new TemplateMatch(match, values);
                    }
                }

                return default(TemplateMatch);
            }

            // Go through different types of matches in precedence order
            if (current.Literals.Count > 0)
            {
                // This code needs to use PathSegment to avoid allocations. I'd recommend a binary search
                // with a list. This is left as an exercise to the reader.
                var segment = enumerator.Current.ToString();

                UrlMatchingNode next;
                if (current.Literals.TryGetValue(segment, out next))
                {
                    var match = Match(context, next, enumerator);
                    if (match != default(TemplateMatch))
                    {
                        return match;
                    }
                }
            }

            if (current.ConstrainedParameters != null)
            {
                var match = Match(context, current.ConstrainedParameters, enumerator);
                if (match != default(TemplateMatch))
                {
                    return match;
                }
            }

            if (current.Parameters != null)
            {
                var match = Match(context, current.Parameters, enumerator);
                if (match != default(TemplateMatch))
                {
                    return match;
                }
            }

            if (current.ConstrainedCatchAlls != null)
            {
                var match = Match(context, current.ConstrainedCatchAlls, enumerator);
                if (match != default(TemplateMatch))
                {
                    return match;
                }
            }

            if (current.CatchAlls != null)
            {
                var match = Match(context, current.CatchAlls, enumerator);
                if (match != default(TemplateMatch))
                {
                    return match;
                }
            }

            return default(TemplateMatch);
        }

        private static void MergeValues(
            IDictionary<string, object> destination,
            IDictionary<string, object> values)
        {
            foreach (var kvp in values)
            {
                // This will replace the original value for the specified key.
                // Values from the matched route will take preference over previous
                // data in the route context.
                destination[kvp.Key] = kvp.Value;
            }
        }

        private struct TemplateMatch : IEquatable<TemplateMatch>
        {
            public TemplateMatch(UrlMatchingEntry entry, IDictionary<string, object> values)
            {
                Entry = entry;
                Values = values;
            }

            public UrlMatchingEntry Entry { get; }

            public IDictionary<string, object> Values { get; }

            public override bool Equals(object obj)
            {
                if (obj is TemplateMatch)
                {
                    return Equals((TemplateMatch)obj);
                }

                return false;
            }

            public bool Equals(TemplateMatch other)
            {
                return
                    object.ReferenceEquals(Entry, other.Entry) &&
                    object.ReferenceEquals(Values, other.Values);
            }

            public override int GetHashCode()
            {
                var hash = new HashCodeCombiner();
                hash.Add(Entry);
                hash.Add(Values);
                return hash.CombinedHash;
            }

            public static bool operator ==(TemplateMatch left, TemplateMatch right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TemplateMatch left, TemplateMatch right)
            {
                return !left.Equals(right);
            }
        }
    }
}
