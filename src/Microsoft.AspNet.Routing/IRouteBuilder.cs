// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;

namespace Microsoft.AspNet.Routing
{
    public interface IRouteBuilder
    {
        IApplicationBuilder ApplicationBuilder { get; }

        IRouter DefaultHandler { get; set; }

        IServiceProvider ServiceProvider { get; }

        IList<IRouter> Routes { get; }

        IRouter Build();
    }
}