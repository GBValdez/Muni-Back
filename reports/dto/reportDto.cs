using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.catalogues;

namespace back.reports.dto
{
    public class reportDto : reportDtoBase
    {
        public Status status { get; set; }
        public catalogues.Type type { get; set; }


    }
}