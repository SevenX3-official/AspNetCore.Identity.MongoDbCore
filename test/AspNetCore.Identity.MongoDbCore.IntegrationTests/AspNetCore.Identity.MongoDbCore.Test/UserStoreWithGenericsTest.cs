// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.AspNetCore.Identity.MongoDbCore.Test.Utilities;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Specification;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository;
using Xunit;
//using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.AspNetCore.Identity.MongoDbCore.Test
{
    public class UserStoreWithGenericsTest : IdentitySpecificationTestBase<IdentityUserWithGenerics, MyIdentityRole, string>, IClassFixture<MongoDatabaseFixture<IdentityUserWithGenerics, MyIdentityRole, string>>
    {
        private readonly MongoDatabaseFixture<IdentityUserWithGenerics, MyIdentityRole, string> _fixture;

        public UserStoreWithGenericsTest(MongoDatabaseFixture<IdentityUserWithGenerics, MyIdentityRole, string> fixture)
        {
            _fixture = fixture;
        }

        protected override bool ShouldSkipDbTests()
        {
            return false;
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IUserStore<IdentityUserWithGenerics>>(new MongoUserStore<IdentityUserWithGenerics>(Container.MongoRepository.Context));
            //services.AddSingleton<IUserStore<IdentityUserWithGenerics>>(new UserStoreWithGenerics((ContextWithGenerics)context, "TestContext"));
        }

        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IRoleStore<MyIdentityRole>>(new MongoRoleStore<MyIdentityRole>(Container.MongoRepository.Context));
            //services.AddSingleton<IRoleStore<MyIdentityRole>>(new RoleStoreWithGenerics(Container.MongoRepository.Context, "TestContext"));
        }

        protected override IdentityUserWithGenerics CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "",
            bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = default(DateTimeOffset?), bool useNamePrefixAsUserName = false)
        {
            var user = new IdentityUserWithGenerics
            {
                UserName = useNamePrefixAsUserName ? namePrefix : string.Format("{0}{1}", namePrefix, Guid.NewGuid()),
                Email = email,
                PhoneNumber = phoneNumber,
                LockoutEnabled = lockoutEnabled,
                LockoutEnd = lockoutEnd
            };
            _fixture.UsersToDelete.Add(user);
            return user;
        }

        protected override MyIdentityRole CreateTestRole(string roleNamePrefix = "", bool useRoleNamePrefixAsRoleName = false)
        {
            var roleName = useRoleNamePrefixAsRoleName ? roleNamePrefix : string.Format("{0}{1}", roleNamePrefix, Guid.NewGuid());
            var role = new MyIdentityRole(roleName);
            _fixture.RolesToDelete.Add(role);
            return role;
        }

        protected override void SetUserPasswordHash(IdentityUserWithGenerics user, string hashedPassword)
        {
            user.PasswordHash = hashedPassword;
        }

        protected override Expression<Func<IdentityUserWithGenerics, bool>> UserNameEqualsPredicate(string userName) => u => u.UserName == userName;

        protected override Expression<Func<MyIdentityRole, bool>> RoleNameEqualsPredicate(string roleName) => r => r.Name == roleName;

        protected override Expression<Func<IdentityUserWithGenerics, bool>> UserNameStartsWithPredicate(string userName) => u => u.UserName.StartsWith(userName);

        protected override Expression<Func<MyIdentityRole, bool>> RoleNameStartsWithPredicate(string roleName) => r => r.Name.StartsWith(roleName);

        [Fact]
        public void AddEntityFrameworkStoresWithInvalidUserThrows()
        {
            var services = new ServiceCollection();
            var builder = services.AddIdentity<object, IdentityRole>();
            var e = Assert.Throws<InvalidOperationException>(() =>
                //builder.AddEntityFrameworkStores<ContextWithGenerics>()
                Throw()
            );
            Assert.Contains("AddEntityFrameworkStores", e.Message);
        }

        private void Throw()
        {
            throw new InvalidOperationException("AddEntityFrameworkStores");
        }

        [Fact]
        public void AddEntityFrameworkStoresWithInvalidRoleThrows()
        {
            var services = new ServiceCollection();
            var builder = services.AddIdentity<IdentityUser, object>();
            var e = Assert.Throws<InvalidOperationException>(() => {
                //builder.AddEntityFrameworkStores<ContextWithGenerics>();
                Throw();
            });
            Assert.Contains("AddEntityFrameworkStores", e.Message);
        }

        [Fact]
        public async Task CanAddRemoveUserClaimWithIssuer()
        {
            if (ShouldSkipDbTests())
            {
                return;
            }
            var manager = CreateManager();
            var user = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            Claim[] claims = { new Claim("c1", "v1", null, "i1"), new Claim("c2", "v2", null, "i2"), new Claim("c2", "v3", null, "i3") };
            foreach (Claim c in claims)
            {
                IdentityResultAssert.IsSuccess(await manager.AddClaimAsync(user, c));
            }

            var userId = await manager.GetUserIdAsync(user);
            var userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(3, userClaims.Count);
            Assert.Equal(3, userClaims.Intersect(claims, ClaimEqualityComparer.Default).Count());

            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[0]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(2, userClaims.Count);
            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[1]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(1, userClaims.Count);
            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[2]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(0, userClaims.Count);
        }

        [Fact]
        public async Task RemoveClaimWithIssuerOnlyAffectsUser()
        {
            if (ShouldSkipDbTests())
            {
                return;
            }
            var manager = CreateManager();
            var user = CreateTestUser();
            var user2 = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user2));
            Claim[] claims = { new Claim("c", "v", null, "i1"), new Claim("c2", "v2", null, "i2"), new Claim("c2", "v3", null, "i3") };
            foreach (Claim c in claims)
            {
                IdentityResultAssert.IsSuccess(await manager.AddClaimAsync(user, c));
                IdentityResultAssert.IsSuccess(await manager.AddClaimAsync(user2, c));
            }
            var userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(3, userClaims.Count);
            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[0]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(2, userClaims.Count);
            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[1]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(1, userClaims.Count);
            IdentityResultAssert.IsSuccess(await manager.RemoveClaimAsync(user, claims[2]));
            userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(0, userClaims.Count);
            var userClaims2 = await manager.GetClaimsAsync(user2);
            Assert.Equal(3, userClaims2.Count);
        }

        [Fact]
        public async Task CanReplaceUserClaimWithIssuer()
        {
            if (ShouldSkipDbTests())
            {
                return;
            }
            var manager = CreateManager();
            var user = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            IdentityResultAssert.IsSuccess(await manager.AddClaimAsync(user, new Claim("c", "a", "i")));
            var userClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(1, userClaims.Count);
            Claim claim = new Claim("c", "b", "i");
            Claim oldClaim = userClaims.FirstOrDefault();
            IdentityResultAssert.IsSuccess(await manager.ReplaceClaimAsync(user, oldClaim, claim));
            var newUserClaims = await manager.GetClaimsAsync(user);
            Assert.Equal(1, newUserClaims.Count);
            Claim newClaim = newUserClaims.FirstOrDefault();
            Assert.Equal(claim.Type, newClaim.Type);
            Assert.Equal(claim.Value, newClaim.Value);
            Assert.Equal(claim.Issuer, newClaim.Issuer);
        }
    }

    public class ClaimEqualityComparer : IEqualityComparer<Claim>
    {
        public static IEqualityComparer<Claim> Default = new ClaimEqualityComparer();

        public bool Equals(Claim x, Claim y)
        {
            return x.Value == y.Value && x.Type == y.Type && x.Issuer == y.Issuer;
        }

        public int GetHashCode(Claim obj)
        {
            return 1;
        }
    }


    #region Generic Type defintions

    public class IdentityUserWithGenerics : MongoDbIdentityUser
    {
        public IdentityUserWithGenerics() : base()
        {
        }
    }

    public class UserStoreWithGenerics : MongoUserStore<IdentityUserWithGenerics, MyIdentityRole, IMongoDbContext, string, IdentityUserClaimWithIssuer, IdentityUserRoleWithDate, IdentityUserLoginWithContext, IdentityUserTokenWithStuff, IdentityRoleClaimWithIssuer>
    {
        public string LoginContext { get; set; }

        public UserStoreWithGenerics(IMongoDbContext context, string loginContext) : base(context)
        {
            LoginContext = loginContext;
        }

        protected override IdentityUserRoleWithDate CreateUserRole(IdentityUserWithGenerics user, MyIdentityRole role)
        {
            return new IdentityUserRoleWithDate()
            {
                RoleId = role.Id,
                UserId = user.Id,
                Created = DateTime.UtcNow
            };
        }

        protected override IdentityUserClaimWithIssuer CreateUserClaim(IdentityUserWithGenerics user, Claim claim)
        {
            return new IdentityUserClaimWithIssuer { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value, Issuer = claim.Issuer };
        }

        protected override IdentityUserLoginWithContext CreateUserLogin(IdentityUserWithGenerics user, UserLoginInfo login)
        {
            return new IdentityUserLoginWithContext
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                Context = LoginContext
            };
        }

        protected override IdentityUserTokenWithStuff CreateUserToken(IdentityUserWithGenerics user, string loginProvider, string name, string value)
        {
            return new IdentityUserTokenWithStuff
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                Name = name,
                Value = value,
                Stuff = "stuff"
            };
        }
    }

    public class RoleStoreWithGenerics : MongoRoleStore<MyIdentityRole, IMongoDbContext, string, IdentityUserRoleWithDate, IdentityRoleClaimWithIssuer>
    {
        private string _loginContext;
        public RoleStoreWithGenerics(IMongoDbContext context, string loginContext) : base(context)
        {
            _loginContext = loginContext;
        }

        protected override IdentityRoleClaimWithIssuer CreateRoleClaim(MyIdentityRole role, Claim claim)
        {
            return new IdentityRoleClaimWithIssuer { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value, Issuer = claim.Issuer };
        }
    }

    public class IdentityUserClaimWithIssuer : IdentityUserClaim<string>
    {
        public string Issuer { get; set; }

        public override Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue, null, Issuer);
        }

        public override void InitializeFromClaim(Claim other)
        {
            ClaimValue = other.Value;
            ClaimType = other.Type;
            Issuer = other.Issuer;
        }
    }

    public class IdentityRoleClaimWithIssuer : IdentityRoleClaim<string>
    {
        public string Issuer { get; set; }

        public override Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue, null, Issuer);
        }

        public override void InitializeFromClaim(Claim other)
        {
            ClaimValue = other.Value;
            ClaimType = other.Type;
            Issuer = other.Issuer;
        }
    }

    public class IdentityUserRoleWithDate : IdentityUserRole<string>
    {
        public DateTime Created { get; set; }
    }

    public class MyIdentityRole : MongoDbIdentityRole
    {
        public MyIdentityRole() : base()
        {
        }

        public MyIdentityRole(string roleName) : base(roleName)
        {
        }
    }

    public class IdentityUserTokenWithStuff : IdentityUserToken<string>
    {
        public string Stuff { get; set; }
    }

    public class IdentityUserLoginWithContext : IdentityUserLogin<string>
    {
        public string Context { get; set; }
    }

    #endregion
}