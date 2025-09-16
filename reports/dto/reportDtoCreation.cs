using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.reports.dto
{
    public class reportDtoCreation : reportDtoBase
    {
        public long statusId { get; set; }
        public long typeId { get; set; }
        public string? userValidationId { get; set; }
    }
}