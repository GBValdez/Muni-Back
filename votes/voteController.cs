using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using back.utils.dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project.Models;

namespace back.votes
{
    [ApiController]
    [Route("votes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class voteController : ControllerBase
    {
        private readonly DBProyContext context;
        public voteController(DBProyContext context)
        {
            this.context = context;
        }
        [HttpPost()]
        public async Task<ActionResult> vote([FromBody] idDtoRequired dtoRequired)
        {
            string idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Votes findVote = await context.Set<Votes>().FirstOrDefaultAsync(v => v.reportId == dtoRequired.Id && v.userCreateId == idUser);
            if (findVote == null)
            {
                Votes voto = new Votes();
                voto.reportId = dtoRequired.Id;
                await context.AddAsync(voto);
            }
            else
            {
                findVote.deleteAt = findVote.deleteAt != null ? null : DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            return Ok();
        }


    }
}