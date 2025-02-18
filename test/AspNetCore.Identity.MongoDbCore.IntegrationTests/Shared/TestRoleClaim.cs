// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.Shared
{
    /// <summary>
    ///     EntityType that represents one specific role claim
    /// </summary>
    public class TestRoleClaim : TestRoleClaim<string> { }

    /// <summary>
    ///     EntityType that represents one specific role claim
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class TestRoleClaim<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     Primary key
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        ///     User Id for the role this claim belongs to
        /// </summary>
        public virtual TKey RoleId { get; set; }

        /// <summary>
        ///     Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        ///     Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}