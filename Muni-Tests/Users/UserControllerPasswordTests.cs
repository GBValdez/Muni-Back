#pragma warning disable CS8625 // Permitimos null en parámetros no anulables en este archivo de test

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using project.Models;
using project.users;
using project.users.Models;
using project.users.dto;

using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MuniBack.Tests.Users
{
    public class UserControllerPasswordTests
    {
        private static IConfiguration TestConfig()
        {
            var dict = new Dictionary<string, string?>
            {
                ["keyJwt"] = "this_is_a_super_secret_jwt_key_for_tests_123456789",
                ["keyResetPasswordKey"] = "reset-key-for-tests"
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        private static DBProyContext InMemoryDb(IConfiguration cfg)
        {
            var opts = new DbContextOptionsBuilder<DBProyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DBProyContext(opts, cfg);
        }

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

        private static userController BuildController(
            Mock<UserManager<userEntity>> userMgrMock,
            Mock<SignInManager<userEntity>> signInMock,
            IConfiguration cfg)
        {
            var db = InMemoryDb(cfg);
            var dataProvider = new NoOpDataProtectionProvider();

            var controller = new userController(
                userMgrMock.Object,
                cfg,
                signInMock.Object,
                emailService: null!, // no se usa aquí
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

        // ============== resetPassword ==============

        [Fact]
        public async Task ResetPassword_Should_Return_Ok_When_Token_Valid()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            var user = new userEntity { Email = "u@x.com", UserName = "ux" };
            userMgr.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);

            string tokenBase = "RESETTOKEN";
            string tokenEncrypt = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenBase));
            string tokenForDto = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenEncrypt));

            userMgr.Setup(m => m.ResetPasswordAsync(user, tokenBase, "New@1234"))
                   .ReturnsAsync(IdentityResult.Success);

            var dto = new resetPasswordDto
            {
                email = Convert.ToBase64String(Encoding.UTF8.GetBytes("u@x.com")),
                token = tokenForDto,
                password = "New@1234"
            };

            var result = await controller.resetPassword(dto);

            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ResetPassword_Should_Return_BadRequest_When_User_NotFound()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            userMgr.Setup(m => m.FindByEmailAsync("no@x.com"))
                   .ReturnsAsync((userEntity?)null);

            var dto = new resetPasswordDto
            {
                email = Convert.ToBase64String(Encoding.UTF8.GetBytes("no@x.com")),
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("whatever")),
                password = "New@1234"
            };

            var result = await controller.resetPassword(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ResetPassword_Should_Return_BadRequest_When_Reset_Fails()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            var user = new userEntity { Email = "u@x.com", UserName = "ux" };
            userMgr.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);

            string tokenBase = "BADTOKEN";
            string tokenEncrypt = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenBase));
            string tokenForDto = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenEncrypt));

            userMgr.Setup(m => m.ResetPasswordAsync(user, tokenBase, "New@1234"))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "invalid token" }));

            var dto = new resetPasswordDto
            {
                email = Convert.ToBase64String(Encoding.UTF8.GetBytes("u@x.com")),
                token = tokenForDto,
                password = "New@1234"
            };

            var result = await controller.resetPassword(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ============== confirmEmail ==============

        [Fact]
        public async Task ConfirmEmail_Should_Return_Ok_When_Token_Valid()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            var user = new userEntity { Email = "u@x.com", UserName = "ux" };
            userMgr.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);

            string rawToken = "CONFIRM_TOKEN";
            string tokenParam = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            userMgr.Setup(m => m.ConfirmEmailAsync(user, rawToken))
                   .ReturnsAsync(IdentityResult.Success);

            var result = await controller.confirmEmail("u@x.com", tokenParam);

            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_User_Not_Found()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            string tokenParam = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("ANY"));

            userMgr.Setup(m => m.FindByEmailAsync("no@x.com"))
                   .ReturnsAsync((userEntity?)null);

            var result = await controller.confirmEmail("no@x.com", tokenParam);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ConfirmEmail_Should_Return_BadRequest_When_Confirm_Fails()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var signIn = MockSignInManager(userMgr.Object);
            var controller = BuildController(userMgr, signIn, cfg);

            var user = new userEntity { Email = "u@x.com", UserName = "ux" };
            userMgr.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);

            string rawToken = "CONFIRM_TOKEN";
            string tokenParam = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            userMgr.Setup(m => m.ConfirmEmailAsync(user, rawToken))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad" }));

            var result = await controller.confirmEmail("u@x.com", tokenParam);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

#pragma warning restore CS8625
