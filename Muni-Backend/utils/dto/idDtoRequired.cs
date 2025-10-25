using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back.utils.dto
{
    public class idDtoRequired
    {
        [Required()]
        public long Id { get; set; }
    }
}