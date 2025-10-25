#pragma warning disable CS8625

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using project.Models;
using project.roles;
using project.roles.dto;
using project.users;
using System.Threading.Tasks;

namespace MuniBack.Tests.Roles
{
    // Subclase para exponer el método protegido validDelete
    public class RolControllerTestable : rolController
    {
        public RolControllerTestable(
            RoleManager<rolEntity> roleManager,
            DBProyContext context,
            IMapper mapper,
            UserManager<userEntity> userManager
        ) : base(roleManager, context, mapper, userManager) { }

        public Task<project.utils.dto.errorMessageDto> ExposeValidDelete(rolEntity entity)
            => base.validDelete(entity);
    }

    public class RolControllerTests
    {
        private static Microsoft.Extensions.Configuration.IConfiguration TestConfig()
        {
            var dict = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["keyJwt"] = "tests",
            };
            return new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        private static DBProyContext InMemoryDb(Microsoft.Extensions.Configuration.IConfiguration cfg)
        {
            var opts = new DbContextOptionsBuilder<DBProyContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;
            return new DBProyContext(opts, cfg);
        }

        private static Mock<UserManager<userEntity>> MockUserManager()
        {
            var store = new Mock<IUserStore<userEntity>>();
            return new Mock<UserManager<userEntity>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<rolEntity>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<rolEntity>>();
            var roleValidators = new System.Collections.Generic.List<IRoleValidator<rolEntity>>();
            var normalizer = new Mock<ILookupNormalizer>();
            var errors = new IdentityErrorDescriber();
            var logger = new Mock<ILogger<RoleManager<rolEntity>>>();
            return new Mock<RoleManager<rolEntity>>(
                store.Object, roleValidators, normalizer.Object, errors, logger.Object);
        }

        [Fact]
        public async Task Put_Should_Return_BadRequest()
        {
            var cfg = TestConfig();
            using var db = InMemoryDb(cfg);

            var roleMgr = MockRoleManager();
            var userMgr = MockUserManager();
            var mapper = new Mock<IMapper>();

            var controller = new rolController(roleMgr.Object, db, mapper.Object, userMgr.Object);

            var dto = new rolCreationDto { name = "ADMINISTRATOR" };
            var result = await controller.put(dto, "ADMINISTRATOR", null);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_Should_Return_Error_When_Role_In_Use()
        {
            var cfg = TestConfig();
            using var db = InMemoryDb(cfg);

            var roleMgr = MockRoleManager();
            var userMgr = MockUserManager();
            var mapper = new Mock<IMapper>();

            userMgr.Setup(m => m.GetUsersInRoleAsync("ADMINISTRATOR"))
                   .ReturnsAsync(new System.Collections.Generic.List<userEntity> { new userEntity() });

            var controller = new RolControllerTestable(roleMgr.Object, db, mapper.Object, userMgr.Object);

            var err = await controller.ExposeValidDelete(new rolEntity { Name = "ADMINISTRATOR" });

            err.Should().NotBeNull(); // mensaje: "No se puede eliminar el rol porque esta siendo usado por un usuario"
        }
    }
}


#pragma warning restore CS8625