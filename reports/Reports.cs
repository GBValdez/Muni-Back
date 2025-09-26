using System.ComponentModel.DataAnnotations.Schema;
using back.catalogues;
using project.users;
using project.utils;

namespace back.reports
{
    public class Reports : CommonsModel<long>
    {
        public string location { get; set; } = null!;
        public string description { get; set; } = null!;
        public long statusId { get; set; }
        public Status status { get; set; }
        public long typeId { get; set; }
        public catalogues.Type type { get; set; }
        public string title = null!;
        public string? reasonForRejection;
        public string? userValidationId { get; set; }

        [ForeignKey("userValidationId")]
        public userEntity? userValidation { get; set; }
    }
}