using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Repo.Repository.Interface
{
    public interface IUserRepository
    {
        string ValidateCredential(string validateBy, string value);
        string ChangePassword(LoginViewModel vmModel);
        bool CreateUser(UserViewModel vmModel);
        string Login(LoginViewModel vmModel);
    }
}
