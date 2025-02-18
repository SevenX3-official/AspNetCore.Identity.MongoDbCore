// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Shared;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.AspNetCore.Identity.MongoDbCore.Test
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly => typeof(IdentityUser).GetTypeInfo().Assembly;
    }
}
