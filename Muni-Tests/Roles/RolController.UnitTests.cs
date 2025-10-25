#pragma warning disable CS8625
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using project.Models;
using project.roles;
using project.roles.dto;
using project.users;
using project.utils.dto;

namespace Muni_Tests.Roles
{
    // Mantengo esta clase auxiliar tal cual
    public class RolControllerTestable : rolController
    {
        public RolControllerTestable(RoleManager<rolEntity> roleManager, DBProyContext context, IMapper mapper, UserManager<userEntity> userManager)
            : base(roleManager, context, mapper, userManager) { }

        public Task<errorMessageDto?> ExposeValidDelete(rolEntity entity) => base.validDelete(entity);
    }

    public class RolControllerUnitTests
    {
        [Fact]
        public async Task validDelete_debe_rechazar_cuando_rol_tiene_usuarios()
        {
            // Arrange
            var db = Muni_Tests.Shared.TestHelpers.BuildInMemoryContext(nameof(validDelete_debe_rechazar_cuando_rol_tiene_usuarios));
            var roleMgr = Muni_Tests.Shared.TestHelpers.MockRoleManager();
            var userMgr = Muni_Tests.Shared.TestHelpers.MockUserManager();
            var mapper = new Mock<IMapper>();

            userMgr.Setup(m => m.GetUsersInRoleAsync("ADMINISTRATOR"))
                   .ReturnsAsync(new List<userEntity> { new userEntity { Id = "u1" } });

            var sut = new RolControllerTestable(roleMgr.Object, db, mapper.Object, userMgr.Object);

            // Act
            var err = await sut.ExposeValidDelete(new rolEntity { Name = "ADMINISTRATOR" });

            // Assert
            err.Should().NotBeNull();
            err!.message.Should().Contain("No se puede eliminar el rol");
        }

        [Fact]
        public async Task validDelete_debe_permitir_cuando_rol_no_tiene_usuarios()
        {
            var db = Muni_Tests.Shared.TestHelpers.BuildInMemoryContext(nameof(validDelete_debe_permitir_cuando_rol_no_tiene_usuarios));
            var roleMgr = Muni_Tests.Shared.TestHelpers.MockRoleManager();
            var userMgr = Muni_Tests.Shared.TestHelpers.MockUserManager();
            var mapper = new Mock<IMapper>();

            userMgr.Setup(m => m.GetUsersInRoleAsync("SUPERVISOR"))
                   .ReturnsAsync(new List<userEntity>()); // vacío

            var sut = new RolControllerTestable(roleMgr.Object, db, mapper.Object, userMgr.Object);

            var err = await sut.ExposeValidDelete(new rolEntity { Name = "SUPERVISOR" });

            err.Should().BeNull();
        }

        [Fact]
        public async Task put_debe_retornar_BadRequest_siempre()
        {
            // Arrange
            var db = Muni_Tests.Shared.TestHelpers.BuildInMemoryContext(nameof(put_debe_retornar_BadRequest_siempre));
            var roleMgr = Muni_Tests.Shared.TestHelpers.MockRoleManager();
            var userMgr = Muni_Tests.Shared.TestHelpers.MockUserManager();
            var mapper = new Mock<IMapper>();
            var sut = new rolController(roleMgr.Object, db, mapper.Object, userMgr.Object);

            // Act
            var result = await sut.put(new rolCreationDto { name = "X" }, id: "role-1", queryCreation: new object());

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().BeOfType<errorMessageDto>();
        }
    }
}

