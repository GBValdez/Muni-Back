

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
    public class ReportsController : controllerCommons<Reports, reportDtoCreation, reportDto, object, object, long>
    {
        public ReportsController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "userNormal,ADMINMUNI,ADMINISTRATOR")]
        public override Task<ActionResult<reportDto>> post(reportDtoCreation newRegister, [FromQuery] object queryParams)
        {
            return base.post(newRegister, queryParams);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public override Task<ActionResult> put(reportDtoCreation entityCurrent, [FromRoute] long id, [FromQuery] object queryCreation)
        {
            return null;
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                    .OrderByDescending(t => t.votes)
                    .ToListAsync();

            List<reportDto> listDto = mapper.Map<List<reportDto>>(reportsWithVotes);
            return new resPag<reportDto>
            {
                items = listDto,
                total = total,
                index = infoQuery.pageNumber,
                totalPages = totalPages
            };
        }

        [HttpGet("unvalidation")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMINMUNI")]
        public async Task<ActionResult<resPag<reportDto>>> reportsValid([FromQuery] pagQueryDto infoQuery, [FromQuery] object queryParams)
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

            query = query.Include(t => t.userCreate).Include(t => t.status).Include(t => t.type).Where(t => t.statusId == 1).Include(t => t.userCreate);

            if (infoQuery.all == false)
                query = query
                .Skip((infoQuery.pageNumber - 1) * infoQuery.pageSize)
                .Take(infoQuery.pageSize);

            List<Reports> reports = await query
                    .ToListAsync();

            List<reportDto> listDto = mapper.Map<List<reportDto>>(reports);
            return new resPag<reportDto>
            {
                items = listDto,
                total = total,
                index = infoQuery.pageNumber,
                totalPages = totalPages
            };
        }


        [HttpPost("request/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMINMUNI,ADMINISTRATOR")]
        public async Task<ActionResult> request([FromRoute] int id, [FromBody] declineReportDto body)
        {
            string idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Reports findReport = await context.Set<Reports>().FirstOrDefaultAsync(v => v.Id == id && v.deleteAt == null);
            if (findReport != null)
            {
                if (body.accepted)
                {
                    findReport.statusId = 3;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(body.reasonForRejection))
                        return BadRequest(new errorMessageDto("La razón es requerida"));
                    findReport.reasonForRejection = body.reasonForRejection;
                    findReport.statusId = 2;
                }
                findReport.userValidationId = idUser;
                await context.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }
            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "userNormal,ADMINMUNI,ADMINISTRATOR")]
        public override Task<ActionResult> delete(long id)
        {
            return base.delete(id);
        }

        protected async override Task<IQueryable<Reports>> modifyDelete(IQueryable<Reports> query)
        {
            string idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            query = query.Where(db => db.userCreateId == idUser);
            return query;
        }

        protected override async Task<errorMessageDto> validPost(Reports entity, reportDtoCreation newRegister, object queryParams)
        {
            entity.statusId = 1;
            return null;
        }


    }
}