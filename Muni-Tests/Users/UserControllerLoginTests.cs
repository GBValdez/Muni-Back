#pragma warning disable CS8625 // Permitimos pasar null a parámetros no anulables en este archivo de test

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using project.Models;
using project.users;
using project.users.Models;
using project.users.dto;

// Alias para evitar ambigüedad con Microsoft.AspNetCore.Mvc.SignInResult
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MuniBack.Tests.Users
{
    public class UserControllerLoginTests
    {
        // ---------- Helpers comunes ----------

        private static DBProyContext InMemoryDb(IConfiguration cfg)
        {
            var opts = new DbContextOptionsBuilder<DBProyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // IMPORTANTE: tu DBProyContext pide IConfiguration en el ctor
            return new DBProyContext(opts, cfg);
        }

        private static IConfiguration TestConfig()
        {
            var dict = new Dictionary<string, string?>
            {
                ["keyJwt"] = "this_is_a_super_secret_jwt_key_for_tests_123456789",
                ["keyResetPasswordKey"] = "reset-key-for-tests"
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        // No-op DataProtection (el controller crea un protector en el ctor)
        private sealed class NoOpProtector : IDataProtector
        {
            public IDataProtector CreateProtector(string purpose) => this;
            public byte[] Protect(byte[] plaintext) => plaintext;
            public byte[] Unprotect(byte[] protectedData) => protectedData;
        }
        private sealed class NoOpDataProtectionProvider : IDataProtectionProvider
        {
            public IDataProtector CreateProtector(string purpose) => new NoOpProtector();
        }

        private static Mock<UserManager<userEntity>> MockUserManager()
        {
            var store = new Mock<IUserStore<userEntity>>();
            return new Mock<UserManager<userEntity>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<SignInManager<userEntity>> MockSignInManager(UserManager<userEntity> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<userEntity>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(o => o.Value).Returns(new IdentityOptions());
            var logger = new Mock<ILogger<SignInManager<userEntity>>>();
            var schemes = new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<userEntity>>();

            return new Mock<SignInManager<userEntity>>(
                userManager,
                contextAccessor.Object,
                claimsFactory.Object,
                options.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);
        }

        private static void StubClaimsAndRoles(
            Mock<UserManager<userEntity>> um, userEntity user,
            IEnumerable<Claim>? claims = null,
            IEnumerable<string>? roles = null)
        {
            um.Setup(m => m.GetClaimsAsync(user))
              .ReturnsAsync((claims ?? Array.Empty<Claim>()).ToList());

            um.Setup(m => m.GetRolesAsync(user))
              .ReturnsAsync((roles ?? Array.Empty<string>()).ToList());
        }

        private static userController BuildController(
            Mock<UserManager<userEntity>> userMgrMock,
            Mock<SignInManager<userEntity>> signInMock,
            IConfiguration cfg)
        {
            var db = InMemoryDb(cfg);
            var dataProvider = new NoOpDataProtectionProvider();

            // emailService, mapper y userSvc: el login NO los usa → pasamos null (y suprimimos warning con #pragma)
            var controller = new userController(
                userMgrMock.Object,
                cfg,
                signInMock.Object,
                emailService: null!,
                dataProvider,
                db,
                mapper: null!,
                userSvc: null!
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        // ---------- TESTS ----------

        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
        {
            // Arrange
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);

            var user = new userEntity
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                UserName = "testuser",
                deleteAt = null
            };

            userMgr.Setup(m => m.FindByEmailAsync("test@example.com"))
                   .ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user))
                   .ReturnsAsync(true);

            // IMPORTANTE: evitar NullReference en createToken()
            StubClaimsAndRoles(userMgr, user); // listas vacías por defecto

            signIn.Setup(s => s.PasswordSignInAsync("testuser", "P@ssw0rd!", false, false))
                  .ReturnsAsync(IdentitySignInResult.Success);

            var controller = BuildController(userMgr, signIn, cfg);

            var creds = new credentialsDto
            {
                Email = "test@example.com",
                password = "P@ssw0rd!"
            };

            // Act
            var result = await controller.login(creds);

            // Assert
            result.Result.Should().BeNull(); // devuelve DTO directo (200 OK)
            result.Value.Should().NotBeNull();
            result.Value!.token.Should().NotBeNullOrWhiteSpace();
            result.Value!.expiration.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Not_Found()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);

            userMgr.Setup(m => m.FindByEmailAsync("no@exists.com"))
                   .ReturnsAsync((userEntity?)null);

            var controller = BuildController(userMgr, signIn, cfg);

            var creds = new credentialsDto { Email = "no@exists.com", password = "x" };

            var result = await controller.login(creds);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_Should_Fail_When_Email_Not_Confirmed()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);

            var user = new userEntity { Email = "u@x.com", UserName = "ux", deleteAt = null };

            userMgr.Setup(m => m.FindByEmailAsync("u@x.com"))
                   .ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user))
                   .ReturnsAsync(false);

            // Aún así, por seguridad devolvemos listas vacías
            StubClaimsAndRoles(userMgr, user);

            var controller = BuildController(userMgr, signIn, cfg);

            var creds = new credentialsDto { Email = "u@x.com", password = "x" };
            var result = await controller.login(creds);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Is_Deleted()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);

            var user = new userEntity
            {
                Email = "del@x.com",
                UserName = "del",
                deleteAt = DateTime.UtcNow // simula usuario eliminado
            };

            userMgr.Setup(m => m.FindByEmailAsync("del@x.com"))
                   .ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user))
                   .ReturnsAsync(true);

            StubClaimsAndRoles(userMgr, user);

            var controller = BuildController(userMgr, signIn, cfg);

            var creds = new credentialsDto { Email = "del@x.com", password = "x" };
            var result = await controller.login(creds);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Login_Should_Fail_When_Bad_Password()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);

            var user = new userEntity { Email = "ok@x.com", UserName = "ok", deleteAt = null };

            userMgr.Setup(m => m.FindByEmailAsync("ok@x.com"))
                   .ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user))
                   .ReturnsAsync(true);

            StubClaimsAndRoles(userMgr, user);

            signIn.Setup(s => s.PasswordSignInAsync("ok", "wrong", false, false))
                  .ReturnsAsync(IdentitySignInResult.Failed);

            var controller = BuildController(userMgr, signIn, cfg);

            var creds = new credentialsDto { Email = "ok@x.com", password = "wrong" };
            var result = await controller.login(creds);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

#pragma warning restore CS8625
