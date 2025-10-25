using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.catalogues;
using project.users.dto;
using project.utils.catalogues.dto;

namespace back.reports.dto
{
    public class reportDto : reportDtoBase
    {
        public long Id { get; set; }
        public catalogueDto status { get; set; }
        public catalogueDto type { get; set; }
        public userDto userCreate { get; set; }
        public int votes { get; set; }
        public bool voteMe { get; set; }

    }
}