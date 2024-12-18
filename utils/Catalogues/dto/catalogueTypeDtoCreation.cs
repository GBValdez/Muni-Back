using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project.utils.Catalogues.dto
{
    public class catalogueTypeDtoCreation
    {
        public string code { get; set; } = null!;

        public string name { get; set; } = null!;

        public string? description { get; set; }
    }
}