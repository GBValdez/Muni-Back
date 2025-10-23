#pragma warning disable CS8625 // Permitimos pasar null a parámetros no anulables en este archivo de test

using AutoMapper;
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
using project.users.dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuniBack.Tests.Users
{
    public class UserController_GetByUserName_Tests
    {
        // ---------- Helpers (idéntico estilo a tus otros tests) ----------

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
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            // IMPORTANTE: tu DBProyContext recibe IConfiguration en el ctor
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
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        private static Mock<SignInManager<userEntity>> MockSignInManager(Mock<UserManager<userEntity>> userMgr)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userClaims = new Mock<IUserClaimsPrincipalFactory<userEntity>>();
            var opts = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<userEntity>>>();
            return new Mock<SignInManager<userEntity>>(userMgr.Object, contextAccessor.Object, userClaims.Object, opts.Object, logger.Object, null, null);
        }

        private static userController BuildSut(
            DBProyContext db,
            IConfiguration cfg,
            Mock<UserManager<userEntity>> userMgr,
            IMapper mapper // usa tu IMapper real o mock
        )
        {
            var signInMock = MockSignInManager(userMgr);
            var dataProvider = new NoOpDataProtectionProvider();

            // El orden del ctor ES:
            // (UserManager, IConfiguration, SignInManager, emailService, IDataProtectionProvider, DBProyContext, IMapper, userSvc)
            return new userController(
                userMgr.Object,
                cfg,
                signInMock.Object,
                emailService: null!,
                dataProvider,
                db,
                mapper,
                userSvc: null!
            );
        }

        // ---------- Tests ----------

        [Fact]
        public async Task Debe_retornar_404_si_usuario_no_existe()
        {
            var cfg = TestConfig();
            using var db = InMemoryDb(cfg);

            var userMgr = MockUserManager();
            userMgr.Setup(m => m.FindByNameAsync("ghost")).ReturnsAsync((userEntity?)null);

            // mapper no se usa cuando retorna 404, puede ir null
            var mapper = new Mock<IMapper>(MockBehavior.Loose).Object;

            var sut = BuildSut(db, cfg, userMgr, mapper);

            var resp = await sut.getByUserName("ghost");

            resp.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Debe_retornar_userDto_con_roles_cuando_existe()
        {
            var cfg = TestConfig();
            using var db = InMemoryDb(cfg);

            var userMgr = MockUserManager();
            var user = new userEntity { Id = "u1", UserName = "emerson", Email = "e@x.com", name = "Emerson" };

            userMgr.Setup(m => m.FindByNameAsync("emerson")).ReturnsAsync(user);
            userMgr.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "ADMINISTRATOR", "ADMINMUNI" });

            // Mock de IMapper -> mapea userEntity -> userDto (sin 'id', tu DTO no lo tiene)
            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<userDto>(It.IsAny<userEntity>()))
                .Returns<userEntity>(u => new userDto
                {
                    userName = u.UserName,
                    email = u.Email,
                    name = u.name,
                    isActive = true,
                    roles = new List<string>() // luego el controller asigna roles reales
                });

            var sut = BuildSut(db, cfg, userMgr, mapperMock.Object);

            var resp = await sut.getByUserName("emerson");

            resp.Result.Should().BeNull(); // 200 OK con body
            resp.Value.Should().NotBeNull();
            resp.Value!.userName.Should().Be("emerson");
            resp.Value.roles.Should().Contain(new[] { "ADMINISTRATOR", "ADMINMUNI" });
        }
    }
}

#pragma warning restore CS8625
