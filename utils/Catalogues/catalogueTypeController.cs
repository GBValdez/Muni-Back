using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.utils.catalogue;
using project.utils.Catalogues.dto;

namespace project.utils.Catalogues
{
    [ApiController]
    [Route("[controller]")]
    public class catalogueTypeController : controllerCommons<catalogueType, catalogueTypeDtoCreation, catalogueTypeDtoCreation, object, object, long>
    {
        public catalogueTypeController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}