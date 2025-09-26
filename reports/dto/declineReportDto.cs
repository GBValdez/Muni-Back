using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back.reports.dto
{
    public class declineReportDto
    {
        [Required(ErrorMessage = "La razón es requerida")]
        public string reasonForRejection { get; set; }
    }
}