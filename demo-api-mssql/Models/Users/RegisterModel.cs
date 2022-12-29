using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class RegisterModel
    {
        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public string fullname { get; set; }

        [Required]
        public string role { get; set; }

        [Required]
        public string organization { get; set; }
    }
}