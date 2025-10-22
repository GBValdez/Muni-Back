
#pragma warning disable CS8625


using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;           // DBProyContext
using back.reports.dto;        // declineReportDto
using back.votes;              // Votes

namespace back.reports
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly DBProyContext context;
        private readonly IMapper mapper;

        public ReportsController(DBProyContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        private string? GetUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // --------------------------------------------------------------------
        // VOTE: crea voto si no existe, o hace toggle de deleteAt si ya existe.
        // Solo permite votar reportes aprobados (statusId == 3).
        // --------------------------------------------------------------------
        [HttpPost("vote/{id:int}")]
        public async Task<ActionResult> vote([FromRoute] int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var report = await context.Set<Reports>()
                                      .FirstOrDefaultAsync(r => r.Id == id && r.deleteAt == null);

            if (report == null)
            {
                return NotFound();
            }

            // (Opcional) No permitir votar tu propio reporte.
            if (report.userCreateId == userId)
            {
                // Responder OK sin crear voto (comportamiento "suave" para no romper clientes).
                return Ok();
            }

            // Reglas de negocio: solo reportes aprobados (statusId == 3) aceptan votos
            if (report.statusId != 3)
            {
                // Mantener respuesta OK, pero no crear/alterar voto.
                return Ok();
            }

            var existing = await context.Set<Votes>()
                                        .FirstOrDefaultAsync(v => v.reportId == id &&
                                                                  v.userCreateId == userId);

            if (existing == null)
            {
                // ⬅️ FIX: asignar userCreateId al crear el voto
                var vote = new Votes
                {
                    reportId = id,
                    userCreateId = userId,
                    deleteAt = null
                };
                await context.AddAsync(vote);
            }
            else
            {
                // Toggle soft-delete
                existing.deleteAt = (existing.deleteAt == null) ? DateTime.UtcNow : (DateTime?)null;
            }

            await context.SaveChangesAsync();
            return Ok();
        }

        // --------------------------------------------------------------------
        // APPROVE: statusId = 3, guarda validador
        // --------------------------------------------------------------------
        [HttpPost("approve/{id:int}")]
        public async Task<ActionResult> approve([FromRoute] int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var report = await context.Set<Reports>()
                                      .FirstOrDefaultAsync(r => r.Id == id && r.deleteAt == null);

            if (report == null)
            {
                return NotFound();
            }

            report.statusId = 3; // aprobado
            report.userValidationId = userId;

            await context.SaveChangesAsync();
            return Ok();
        }

        // --------------------------------------------------------------------
        // DECLINE: statusId = 2, guarda motivo y validador
        // --------------------------------------------------------------------
        [HttpPost("decline/{id:int}")]
        public async Task<ActionResult> decline([FromRoute] int id, [FromBody] declineReportDto body)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var report = await context.Set<Reports>()
                                      .FirstOrDefaultAsync(r => r.Id == id && r.deleteAt == null);

            if (report == null)
            {
                return NotFound();
            }

            report.statusId = 2; // rechazado
            report.userValidationId = userId;
            report.reasonForRejection = body?.reasonForRejection;

            await context.SaveChangesAsync();
            return Ok();
        }

        // --------------------------------------------------------------------
        // DELETE (soft): solo el dueño puede marcar deleteAt
        // --------------------------------------------------------------------
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> delete([FromRoute] int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var report = await context.Set<Reports>()
                                      .FirstOrDefaultAsync(r => r.Id == id && r.deleteAt == null);

            if (report == null)
            {
                return NotFound();
            }

            if (!string.Equals(report.userCreateId, userId, StringComparison.Ordinal))
            {
                return BadRequest("El reporte no pertenece al usuario autenticado.");
            }

            report.deleteAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}



#pragma warning restore CS8625