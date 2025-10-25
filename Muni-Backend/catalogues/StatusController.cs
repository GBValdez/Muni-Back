using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.utils.catalogues;

namespace back.catalogues
{
    [ApiController]
    [Route("status")]
    public class StatusController : cataloguesController<Status>
    {
        public StatusController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}