// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing
{
    public class DefaultRouterFeature : IRouterFeature
    {
        public RouteData RouteData { get; set; }

        public Func<string, object, PathString> UrlGenerator { get; set; }
    }
}
