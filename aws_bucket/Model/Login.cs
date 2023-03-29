using System.ComponentModel.DataAnnotations;

namespace aws_bucket
{
    public class Login
    {
        [Required(ErrorMessage = "User name is  Required! ")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "User name is  Required! ")]
        public string? Password { get; set; }
       

    }
}
