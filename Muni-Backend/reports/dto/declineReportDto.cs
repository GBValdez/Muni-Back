using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back.reports.dto
{
    public class declineReportDto
    {
        public bool accepted { get; set; }
        public string reasonForRejection { get; set; }
    }
}