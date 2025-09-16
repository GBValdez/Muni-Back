using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.reports.dto
{
    public class reportDtoBase
    {
        public string location { get; set; } = null!;
        public string description { get; set; } = null!;
        public string title = null!;
        public bool accepted;
    }
}