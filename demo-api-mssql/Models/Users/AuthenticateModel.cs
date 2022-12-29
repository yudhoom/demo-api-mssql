using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class AuthenticateModel
    {
        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
    public class ForgotPasswordModel
    {
        [Required]
        public string email { get; set; }

    }
}