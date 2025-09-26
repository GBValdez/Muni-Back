

using System.Reflection.Metadata;
using System.Security.Claims;
using AutoMapper;
using back.catalogues;
using back.reports.dto;
using back.votes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.utils;
using project.utils.dto;

namespace back.reports
{
    [ApiController]
    [Route("reports")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMINISTRATOR,userNormal")]
    public class ReportsController : controllerCommons<Reports, reportDtoCreation, reportDto, object, object, long>
    {
        public ReportsController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override Task<ActionResult<reportDto>> post(reportDtoCreation newRegister, [FromQuery] object queryParams)
        {
            return base.post(newRegister, queryParams);
        }
        public override Task<ActionResult> put(reportDtoCreation entityCurrent, [FromRoute] long id, [FromQuery] object queryCreation)
        {
            return null;
        }


        public override async Task<ActionResult<resPag<reportDto>>> get([FromQuery] pagQueryDto infoQuery, [FromQuery] object queryParams)
        {
            IQueryable<Reports> query = context.Set<Reports>();
            if (!showDeleted)
                query = query.Where(db => db.deleteAt == null);

            int total = await query.CountAsync();

            if (total == 0)
            {

                return new resPag<reportDto>
                {
                    items = new List<reportDto>(),
                    total = 0,
                    index = 0,
                    totalPages = 0
                };
            }

            int totalPages = (int)Math.Ceiling((double)total / infoQuery.pageSize);

            if (infoQuery.pageNumber > totalPages && !infoQuery.all.Value)
                return BadRequest(new errorMessageDto("El indice de la pagina es mayor que el numero de paginas total"));

            if (infoQuery.pageNumber < 0 && !infoQuery.all.Value)
                return BadRequest(new errorMessageDto("El indice de la pagina no puede ser menor que 0"));

            query = query.Include(t => t.userCreate).Include(t => t.status).Include(t => t.type).Where(t => t.statusId == 3).Include(t => t.userCreate);

            if (infoQuery.all == false)
                query = query
                .Skip((infoQuery.pageNumber - 1) * infoQuery.pageSize)
                .Take(infoQuery.pageSize);

            string idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Reports> reportsWithVotes = await query
                    .GroupJoin(
                        context.Set<Votes>(),
                        report => report.Id,
                        vote => vote.reportId,
                        (report, votes) => new
                        {
                            Report = report,
                            VotesCount = votes.Where(v => v.deleteAt == null)
                        })
                    .Select(r => new Reports
                    {
                        Id = r.Report.Id,
                        location = r.Report.location,
                        description = r.Report.description,
                        title = r.Report.title,
                        status = r.Report.status,
                        type = r.Report.type,
                        userCreate = r.Report.userCreate,
                        votes = r.VotesCount.Count(),
                        voteMe = r.VotesCount.Where(v => v.userCreateId == idUser).Any()
                    })
                    .ToListAsync();

            // List<Reports> listDb = await
            //     query
            //     .ToListAsync();
            List<reportDto> listDto = mapper.Map<List<reportDto>>(reportsWithVotes);
            return new resPag<reportDto>
            {
                items = listDto,
                total = total,
                index = infoQuery.pageNumber,
                totalPages = totalPages
            };
        }

        [HttpPost("vote/{id}")]
        public async Task<ActionResult> vote([FromRoute] int id)
        {
            string idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Votes findVote = await context.Set<Votes>().FirstOrDefaultAsync(v => v.reportId == id && v.userCreateId == idUser);
            if (findVote == null)
            {
                Votes voto = new Votes();
                voto.reportId = id;
                await context.AddAsync(voto);
            }
            else
            {
                findVote.deleteAt = findVote.deleteAt != null ? null : DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            return Ok();
        }

        protected override async Task<errorMessageDto> validPost(Reports entity, reportDtoCreation newRegister, object queryParams)
        {
            entity.statusId = 1;
            return null;
        }


    }
}