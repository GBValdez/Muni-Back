#pragma warning disable CS8625
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using project.Models;
using project.roles;
using project.users;

namespace Muni_Tests.Shared;

public static class TestHelpers
{
    public static DBProyContext BuildInMemoryContext(string dbName)
    {
        
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["keyJwt"] = "unit-tests-jwt",
                ["keyResetPasswordKey"] = "unit-tests-reset"
            })
            .Build();

        var opts = new DbContextOptionsBuilder<DBProyContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new DBProyContext(opts, cfg);
    }

    public static ClaimsPrincipal BuildUserPrincipal(string userId = "user-1")
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
    }

    public static Mock<UserManager<userEntity>> MockUserManager()
    {
        var store = new Mock<IUserStore<userEntity>>();
        return new Mock<UserManager<userEntity>>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }

    public static Mock<RoleManager<rolEntity>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<rolEntity>>();
        return new Mock<RoleManager<rolEntity>>(
            store.Object, null, null, null, null
        );
    }

    // DataProtection no-op (para constructores que lo pidan)
    private sealed class NoOpProtector : IDataProtector
    {
        public IDataProtector CreateProtector(string purpose) => this;
        public byte[] Protect(byte[] plaintext) => plaintext;
        public byte[] Unprotect(byte[] protectedData) => protectedData;
    }
    public sealed class NoOpDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtector CreateProtector(string purpose) => new NoOpProtector();
    }

    public static DefaultHttpContext HttpContextWithUser(string userId = "user-1")
    {
        var http = new DefaultHttpContext();
        http.User = BuildUserPrincipal(userId);
        return http;
    }
}
