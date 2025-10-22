using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.utils.catalogues;

namespace back.catalogues
{
    [ApiController]
    [Route("type")]
    public class TypeController : cataloguesController<Type>
    {
        public TypeController(DBProyContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}