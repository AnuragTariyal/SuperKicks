using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SuperKicks.Data.Models;
using SuperKicks.Repo.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperKicks.Repo.Repository
{
    public class UserRepository(ApplicationDbContext context, IPasswordHasher passwordHasher) : IUserRepository
    {
        private readonly ApplicationDbContext _db = context;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        public string CreateUser(string userName, string password)
        {
            var userExists = _db.Users.Where(x => x.Email.Equals(userName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (userExists != null)
            {
                return $"user with {userName} email is alreay exists";
            }
            else
            {
                //Add User
                User user = new()
                {
                    Email = userName,
                    UserName = userName,
                    EmailConfirmed = false,
                    PasswordHash = _passwordHasher.HashPassword(password)
                };
                _db.Users.Add(user);

                //Assign Role
                //var roleID=_db.Roles.Where(x => x.Name==
                //_db.SaveChanges();
                return "";
                
            }
        }
    }
}
