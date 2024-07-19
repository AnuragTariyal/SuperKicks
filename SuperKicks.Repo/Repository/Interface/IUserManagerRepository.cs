using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Repo.Repository.Interface
{
    public interface IUserManagerRepository
    {
        string ValidateCredential(string validateBy, string value);
        string ChangePassword(LoginViewModel viewModel);
        bool CreateUser(UserViewModel viewModel);
        string Login(LoginViewModel viewModel);
    }
}
