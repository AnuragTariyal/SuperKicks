using System.ComponentModel.DataAnnotations;

namespace SuperKicks.Repo.ViewModels
{
    public class UserViewModel
    {
        public Guid? Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
    public class LoginViewModel
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? NewPassword { get; set; }
    }

}
