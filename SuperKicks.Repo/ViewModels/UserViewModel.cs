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

    public class RoleViewModel
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CraetedDateTime { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }
    }

    public class UserRoleViewModel
    {
        public Guid UserId { get; set; }
        public required List<Guid> RoleIds { get; set; }
        public bool AssingUnAssignRoles { get; set; }
    }
}
