using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.reports;
using project.utils;

namespace back.votes
{
    public class Votes : CommonsModel<long>
    {
        public long reportId { get; set; }
        public Reports report { get; set; }
    }
}