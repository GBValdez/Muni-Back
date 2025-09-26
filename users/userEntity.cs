using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using project.utils.interfaces;

namespace project.users
{
    public class userEntity : IdentityUser, ICommonModel<string>
    {
        public string? userUpdateId { get; set; }
        public DateTime? deleteAt { get; set; }
        [ForeignKey("userUpdateId")]
        public userEntity? userUpdate { get; set; }
        public DateTime? createAt { get; set; }
        public DateTime? updateAt { get; set; }
        public string address { get; set; }
        public string dpi { get; set; }
        public string name { get; set; }
        public DateOnly birthdate { get; set; }
        public string? userCreateId { get; set; }
        [ForeignKey("userCreateId")]
        public userEntity? userCreate { get; set; }

    }
}