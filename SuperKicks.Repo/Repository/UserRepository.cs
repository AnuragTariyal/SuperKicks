using SuperKicks.Data.Models;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;
using System.Security.Cryptography;

namespace SuperKicks.Repo.Repository
{
    public class UserRepository(ApplicationDbContext context) : IUserRepository
    {
        private readonly ApplicationDbContext _db = context;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private const int UserLogIn = 1;

        //USER ----------------------------------------------------
        #region PasswordHasingAndSalt
        public string CreateHashPassword(string password)
        {
            using var rng = new RNGCryptoServiceProvider();

            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] key = deriveBytes.GetBytes(KeySize);
                byte[] hashBytes = new byte[SaltSize + KeySize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(key, 0, hashBytes, SaltSize, KeySize);

                return Convert.ToBase64String(hashBytes);
            }

        }
        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            using var deriveBytes = new Rfc2898DeriveBytes(providedPassword, salt, 10000);
            byte[] key = deriveBytes.GetBytes(KeySize);
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[SaltSize + i] != key[i])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
        public string ValidateCredential(string validateBy, string value)
        {
            var normalizedValue = value.ToUpper();
            bool userExists = false;
            if (validateBy == "emailphone")
            {
                userExists = _db.Users.Any(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue);
            }
            else if (validateBy == "username")
            {
                userExists = _db.Users.Any(x => x.NormalizedUserName == normalizedValue);
            }
            return userExists ? $"User with {value} already exists!" : "Success";
        }
        public bool CreateUser(UserViewModel vmModel)
        {
            var userExists = _db.Users.FirstOrDefault(x => x.NormalizedEmail == vmModel.UserName.ToUpper());
            if (userExists is not null)
            {
                return false;
            }
            else
            {
                var appuserID = _db.Users.OrderByDescending(x => x.AppUserId).Select(x => x.AppUserId).FirstOrDefault();
                appuserID = appuserID == 0 ? 1 : appuserID + 1;
                //Add User
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Email = vmModel.Email,
                    NormalizedEmail = vmModel.Email.ToUpper(),
                    UserName = vmModel.UserName,
                    NormalizedUserName = vmModel.UserName.ToUpper(),
                    PhoneNumber = vmModel.PhoneNumber,
                    PhoneNumberConfirmed = false,
                    AppUserId = appuserID,
                    EmailConfirmed = false,
                    CreatedBy = UserLogIn,
                    CraetedDateTime = DateTimeOffset.Now,
                    PasswordHash = CreateHashPassword(vmModel.Password)
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
                return true;
            }
        }
        public bool Login(LoginViewModel vmModel)
        {
            string normalizedValue = vmModel.UserName.ToLower();
            var userExists = _db.Users.Where(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue
                                            || x.NormalizedUserName == normalizedValue).FirstOrDefault();
            if (userExists is not null)
            {
                bool password = VerifyHashedPassword(userExists.PasswordHash, vmModel.Password);
                return password;
            }
            return false;
        }
        public bool ChangePassword(LoginViewModel vmModel)
        {
            bool credential = Login(vmModel);
            if (credential && vmModel.NewPassword is not null)
            {
                string normalizedValue = vmModel.UserName.ToLower();
                var newPass= CreateHashPassword(vmModel.NewPassword);
                var userExists = _db.Users.Where(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue
                                            || x.NormalizedUserName == normalizedValue).FirstOrDefault();
                if (userExists is not null)
                {
                    userExists.PasswordHash = newPass;
                    _db.SaveChanges();
                    return true;
                }
            }
            return false;
        }
    }
}
