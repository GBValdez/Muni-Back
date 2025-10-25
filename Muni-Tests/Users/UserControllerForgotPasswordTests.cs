#pragma warning disable CS8625 // Permitimos null en parámetros no anulables en este archivo de test

using System;
using System.Collections.Generic;
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
using project.utils.dto; // <-- emailDto

namespace MuniBack.Tests.Users
{
    public class ReportsControllerestsTests
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

        private static userController BuildController(
            Mock<UserManager<userEntity>> userMgrMock,
            IConfiguration cfg)
        {
            var db = InMemoryDb(cfg);
            var dataProvider = new NoOpDataProtectionProvider();

            // IMPORTANTE: usa ARGUMENTOS POSICIONALES (el orden del ctor es):
            // UserManager, IConfiguration, SignInManager, emailService, IDataProtectionProvider, DBProyContext, IMapper, userSvc
            var controller = new userController(
                userMgrMock.Object,  // UserManager
                cfg,                 // IConfiguration
                null!,               // SignInManager<userEntity> (no se usa aquí)
                null!,               // emailService (no se usa en estos casos negativos)
                dataProvider,        // IDataProtectionProvider
                db,                  // DBProyContext
                null!,               // IMapper
                null!                // userSvc
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_NoContent_When_User_Not_Found()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var controller = BuildController(userMgr, cfg);

            userMgr.Setup(m => m.FindByEmailAsync("no@x.com"))
                   .ReturnsAsync((userEntity?)null);

            var dto = new emailDto { email = "no@x.com" };

            var result = await controller.forgotPassword(dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_NoContent_When_Email_Not_Confirmed()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var controller = BuildController(userMgr, cfg);

            var user = new userEntity { Email = "u@x.com", UserName = "ux", deleteAt = null };
            userMgr.Setup(m => m.FindByEmailAsync("u@x.com")).ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            var dto = new emailDto { email = "u@x.com" };

            var result = await controller.forgotPassword(dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_NoContent_When_User_Is_Deleted()
        {
            var cfg = TestConfig();
            var userMgr = MockUserManager();
            var controller = BuildController(userMgr, cfg);

            var user = new userEntity { Email = "del@x.com", UserName = "del", deleteAt = DateTime.UtcNow };
            userMgr.Setup(m => m.FindByEmailAsync("del@x.com")).ReturnsAsync(user);
            userMgr.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);

            var dto = new emailDto { email = "del@x.com" };

            var result = await controller.forgotPassword(dto);

            result.Should().BeOfType<NoContentResult>();
        }
    }
}

#pragma warning restore CS8625
