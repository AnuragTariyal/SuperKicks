using Microsoft.AspNetCore.Identity;
using SuperKicks.Data.Models;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;
using System.Security.Cryptography;

namespace SuperKicks.Repo.Repository
{
    public class UserRepository(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager
        , UserManager<ApplicationUser> userManager) : IUserRepository
    {
        private readonly ApplicationDbContext _db = context;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        //USER ----------------------------------------------------
        public string CreateUser(UserViewModel vModel)
        {
            var userExists = _db.Users.Where(x => x.UserName.ToLower() == vModel.UserName.ToLower()).FirstOrDefault();
            if (userExists != null)
            {
                return $"user with {vModel.UserName} email is alreay exists!";
            }
            else
            {
                var appuserID = _db.Users.OrderByDescending(x => x.AppUserId).Select(x => x.AppUserId).FirstOrDefault();
                appuserID = appuserID == 0 ? 1 : appuserID + 1;
                //Add User
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Email = vModel.UserName,
                    UserName = vModel.UserName,
                    AppUserId = appuserID,
                    EmailConfirmed = false,
                    CreatedBy = appuserID,
                    CraetedDateTime = DateTimeOffset.Now,
                    PasswordHash = CreateHashPassword(vModel.Password)//_passwordHasher.HashPassword(vModel.Password)
                };
                _db.Users.Add(user);

                //Assign Role
                var roleID = _db.Roles.Where(x => x.Name == Utility.HDUser).Select(x => x.Id).FirstOrDefault();
                UserRole userRole = new()
                {
                    UserId = user.Id,
                    RoleId = roleID,
                    CraetedDateTime = DateTimeOffset.Now,
                    CreatedBy = appuserID
                };

                _db.SaveChanges();
                return $"user with {vModel.UserName} is created successfully.";
            }
        }

        public string CreateHashPassword(string password)
        {
            // Generate a salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(32);

            // Combine the salt and password bytes for later use
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            // Convert to base64
            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        public async Task<string> Login(UserViewModel vModel)
        {
            var user = await _userManager.FindByEmailAsync(vModel.UserName);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(vModel.UserName, vModel.Password, false, false);
                if (result.Succeeded)
                    return "Login successfull";
                else
                    return "Invalid username or password!";
            }
            return "User not found";
        }

    }
}
