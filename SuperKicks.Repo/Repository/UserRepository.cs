using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SuperKicks.Data.Models;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SuperKicks.Repo.Repository
{
    public class UserRepository(ApplicationDbContext context, IConfiguration config) : IUserRepository
    {
        private readonly IConfiguration _config = config;
        private readonly ApplicationDbContext _db = context;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private const int UserLogIn = 1;

        #region UserController
        #region PasswordHasingAndSalt
        private static string CreateHashPassword(string password)
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
        private static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
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
        public string GenerateToken(string userName)
        {
            string assignRole = string.Join(",", _db.UserRoles.Where(x => x.User.NormalizedUserName == userName.ToUpper()).Select(x=>x.Role.Name).ToList());

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, userName),
                new(ClaimTypes.Role, assignRole)
            };

            var key = _config.GetSection("Jwt:Key").Value;
            if (string.IsNullOrEmpty(key) || key.Length < 64)
            {
                throw new ArgumentException("The key must be at least 64 characters long.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signinCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToken = new JwtSecurityToken(
                issuer: _config.GetSection("Jwt:Issuer").Value,
                audience: _config.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: signinCred);

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }
        public string ValidateCredential(string validateBy, string value)
        {
            var normalizedValue = value.ToUpper();
            bool userExists = false;
            
            switch (validateBy)
            {
                case "emailphone":
                    userExists = _db.Users.Any(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue);
                    break;
                case "username":
                    userExists = _db.Users.Any(x => x.NormalizedUserName == normalizedValue);
                    break;
            }
            return userExists ? $"User with {value} already exists!" : "Success";
        }
        private bool CheckUsenamePassword(LoginViewModel vmModel)
        {
            string normalizedValue = vmModel.UserName.ToUpper();
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
            bool credential = CheckUsenamePassword(vmModel);
            if (credential && vmModel.NewPassword is not null)
            {
                string normalizedValue = vmModel.UserName.ToUpper();
                var newPass = CreateHashPassword(vmModel.NewPassword);
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
                _db.UserRoles.Add(userRole);
                _db.SaveChanges();
                return true;
            }
        }
        public string Login(LoginViewModel vmModel)
        {
            bool validateUser = CheckUsenamePassword(vmModel);
            if (validateUser)
            {
                string token = GenerateToken(vmModel.UserName);
                return token;
            }
            return string.Empty;
        }
        #endregion
    }
}
