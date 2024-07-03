using SuperKicks.Repo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperKicks.Repo.Repository.Interface
{
    public interface IUserRepository
    {
        string CreateHashPassword(string password);
        string CreateUser(UserViewModel vModel);
    }
}
