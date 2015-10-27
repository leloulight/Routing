// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNet.Routing.Tree
{
    public class TreeRouteBuilder
    {
        private readonly List<UrlGeneratingEntry> _generatingEntries;
        private readonly List<UrlMatchingEntry> _matchingEntries;

        public void Add(UrlGeneratingEntry entry)
        {
            _generatingEntries.Add(entry);
        }

        public void Add(UrlMatchingEntry entry)
        {
            _matchingEntries.Add(entry);
        }

        public IRouter Build()
        {
            var trees = new Dictionary<int, UrlMatchingTree>();

            foreach (var entry in _matchingEntries)
            {
                UrlMatchingTree tree;
                if (!trees.TryGetValue(entry.Order, out tree))
                {
                    tree = new UrlMatchingTree();
                    trees.Add(entry.Order, tree);
                }
            }

            return null;
        }

        public void Clear()
        {
            _generatingEntries.Clear();
            _matchingEntries.Clear();
        }

        private void AddEntryToTree(UrlMatchingTree tree, UrlMatchingEntry entry)
        {
            var current = tree.Root;
            for (var i = 0; i < entry.RouteTemplate.Segments.Count; i++)
            {
                var segment = entry.RouteTemplate.Segments[i];
                if (!segment.IsSimple)
                {
                    // Treat complex segments as a constrained parameter
                    if (current.ConstrainedParameters == null)
                    {
                        current.ConstrainedParameters = new UrlMatchingNode(length: i + 1);
                    }

                    current = current.ConstrainedParameters;
                    continue;
                }

                Debug.Assert(segment.Parts.Count == 1);
                var part = segment.Parts[0];
                if (part.IsLiteral)
                {
                    UrlMatchingNode next;
                    if (!current.Literals.TryGetValue(part.Text, out next))
                    {
                        next = new UrlMatchingNode(i + 1);
                        current.Literals.Add(part.Text, next);
                    }

                    current = next;
                    continue;
                }

                if (part.IsParameter && part.InlineConstraints.Any() && !part.IsCatchAll)
                {
                    if (current.ConstrainedParameters == null)
                    {
                        current.ConstrainedParameters = new UrlMatchingNode(length: i + 1);
                    }

                    current = current.ConstrainedParameters;
                    continue;
                }

                if (part.IsParameter && !part.IsCatchAll)
                {
                    if (current.Parameters == null)
                    {
                        current.Parameters = new UrlMatchingNode(length: i + 1);
                    }

                    current = current.Parameters;
                    continue;
                }

                if (part.IsParameter && part.InlineConstraints.Any() && part.IsCatchAll)
                {
                    if (current.ConstrainedCatchAlls == null)
                    {
                        current.ConstrainedCatchAlls = new UrlMatchingNode(length: i + 1);
                    }

                    current = current.ConstrainedCatchAlls;
                    continue;
                }

                if (part.IsParameter && part.IsCatchAll)
                {
                    if (current.CatchAlls == null)
                    {
                        current.CatchAlls = new UrlMatchingNode(length: i + 1);
                    }

                    current = current.CatchAlls;
                    continue;
                }

                Debug.Fail("We shouldn't get here.");
            }

            current.Matches.Add(entry);
        }
    }
}
