// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using AspNetCore.Identity.MongoDbCore.IntegrationTests.AspNetCore.Identity.MongoDbCore.Test.Utilities;
using AspNetCore.Identity.MongoDbCore.Models;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.AspNetCore.Identity.MongoDbCore.Test
{
    public class StringUser : MongoDbIdentityUser
    {
        public StringUser() : base()
        {
        }
    }

    public class StringRole : MongoDbIdentityRole
    {
        public StringRole() : base()
        {
        }
    }

    public class UserStoreStringKeyTest : MongoDbStoreTestBase<StringUser, StringRole, string>
    {
        public UserStoreStringKeyTest(MongoDatabaseFixture<StringUser, StringRole, string> fixture)
            : base(fixture)
        { }

    }
}