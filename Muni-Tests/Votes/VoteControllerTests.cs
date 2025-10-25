#pragma warning disable CS8625
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using back.reports;
using back.votes;
using back.utils.dto;
using Muni_Tests.Shared;

public class VoteControllerTests
{
    private static (voteController sut, DBProyContext db) BuildSut(string dbName, string userId = "user-1")
    {
        var db = TestHelpers.BuildInMemoryContext(dbName);

        // Semilla: reporte existente
        var report = new Reports { Id = 100, title = "Bache", description = "Grande", location = "Zona 1", userCreateId = "otro", statusId = 1, typeId = 1 };
        db.Reports.Add(report);
        db.SaveChanges();

        var sut = new voteController(db);
        sut.ControllerContext = new ControllerContext { HttpContext = TestHelpers.HttpContextWithUser(userId) };
        return (sut, db);
    }

    [Fact]
    public async Task vote_debe_crear_voto_si_no_existe()
    {
        var (sut, db) = BuildSut(nameof(vote_debe_crear_voto_si_no_existe));
        var dto = new idDtoRequired { Id = 100 };

        var result = await sut.vote(dto);

        result.Should().BeOfType<OkResult>();
        var v = await db.Votes.SingleAsync();
        v.reportId.Should().Be(100);
        v.deleteAt.Should().BeNull();
    }

    [Fact]
    public async Task vote_debe_softdelete_si_ya_existe_activo()
    {
        var (sut, db) = BuildSut(nameof(vote_debe_softdelete_si_ya_existe_activo));
        db.Votes.Add(new Votes { reportId = 100, userCreateId = "user-1", createAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = await sut.vote(new idDtoRequired { Id = 100 });

        result.Should().BeOfType<OkResult>();
        var v = await db.Votes.SingleAsync();
        v.deleteAt.Should().NotBeNull();
    }

    [Fact]
    public async Task vote_debe_restaurar_si_estaba_softdeleted()
    {
        var (sut, db) = BuildSut(nameof(vote_debe_restaurar_si_estaba_softdeleted));
        db.Votes.Add(new Votes { reportId = 100, userCreateId = "user-1", createAt = DateTime.UtcNow, deleteAt = DateTime.UtcNow.AddMinutes(-5) });
        await db.SaveChangesAsync();

        var result = await sut.vote(new idDtoRequired { Id = 100 });

        result.Should().BeOfType<OkResult>();
        var v = await db.Votes.SingleAsync();
        v.deleteAt.Should().BeNull();
    }
}

