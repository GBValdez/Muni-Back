

using AutoMapper;
using back.reports.dto;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.utils;

namespace back.reports
{
    [ApiController]
    [Route("reports")]
    public class ReportsController : controllerCommons<Reports, reportDtoCreation, reportDto, object, object, long>
    {
        public ReportsController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}