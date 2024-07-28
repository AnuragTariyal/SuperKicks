using SuperKicks.Data.Models;
using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Repo.Repository.Interface
{
    public interface IUserManagerRepository
    {
        #region UserAndLogin
        List<User> GetUsers();
        string ValidateCredential(string validateBy, string value);
        string ChangePassword(LoginViewModel viewModel);
        string CreateUser(UserViewModel viewModel);
        string Login(LoginViewModel viewModel);
        #endregion

        #region RoleAndUserRole
        List<Role> GetRoles();
        string AddUpdRole(RoleViewModel viewModel);
        string ActiveInactiveRole(bool flag, Guid id);
        #endregion
    }
}
