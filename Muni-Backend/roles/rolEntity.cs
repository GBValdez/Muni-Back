using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using project.users;
using project.utils.interfaces;

namespace project.roles
{
    public class rolEntity : IdentityRole, ICommonModel<string>
    {
        public string? userUpdateId { get; set; }

        public DateTime? deleteAt { get; set; }
        [ForeignKey("userUpdateId")]
        public userEntity? userUpdate { get; set; }
        public DateTime? createAt { get; set; }
        public DateTime? updateAt { get; set; }
        public string? userCreateId { get; set; }
        [ForeignKey("userCreateId")]
        public userEntity? userCreate { get; set; }
    }
}