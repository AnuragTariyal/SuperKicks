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
    public class UserManagerRepository(ApplicationDbContext context, IConfiguration config) : IUserManagerRepository
    {
        private readonly IConfiguration _config = config;
        private readonly ApplicationDbContext _db = context;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        #region UserAndLogin

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
            string normalizedUserName = userName.ToUpper();
            string assignRole = string.Join(",", _db.UserRoles.Where(x => x.User.NormalizedUserName == normalizedUserName)
                .Select(x => x.Role.Name).ToList());

            int appUserID = _db.Users.Where(x => x.NormalizedUserName == normalizedUserName).Select(x => x.AppUserId).FirstOrDefault();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, userName),
                new(ClaimTypes.Role, assignRole),
                new("AppUserID",appUserID.ToString())
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
        public List<User> GetUsers()
        {
            return [.. _db.Users];
        }
        public string ValidateCredential(string validateBy, string value)
        {
            var normalizedValue = value.ToUpper();

            bool userExists = validateBy switch
            {
                "emailphone" => _db.Users.Any(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue),
                "username" => _db.Users.Any(x => x.NormalizedUserName == normalizedValue),
                _ => false
            };
            return userExists ? $"User with {value} already exists!" : "Success";
        }
        private string CheckUsenamePassword(LoginViewModel viewModel)
        {
            string normalizedValue = viewModel.UserName.ToUpper();
            var userExists = _db.Users.Where(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue
                                            || x.NormalizedUserName == normalizedValue).FirstOrDefault();

            if (userExists is null) return $"User not found with {viewModel.UserName}";

            if (userExists.LockoutEnabled && userExists.LockoutEnd > DateTime.Now)
                return $"user is locked please try after {userExists.LockoutEnd.Value:dd/MM/yyyy - hh:mm - t}!";

            bool password = VerifyHashedPassword(userExists.PasswordHash, viewModel.Password);
            if (!password)
            {
                userExists.AccessFailedCount++;
                if (userExists.AccessFailedCount > 2)
                {
                    userExists.LockoutEnabled = true;
                    userExists.LockoutEnd = DateTimeOffset.Now.AddSeconds(40);
                    _db.SaveChanges();
                    return $"User is locked for 40 sec.";
                }
                _db.SaveChanges();
                return "Please enter correct password!";
            }

            userExists.AccessFailedCount = 0;
            userExists.LockoutEnabled = false;
            userExists.LockoutEnd = null;
            _db.SaveChanges();
            return StatusName.Success;
        }
        public string CreateUser(UserViewModel viewModel)
        {
            var userExists = _db.Users.Where(x => x.NormalizedEmail == viewModel.Email.ToUpper() || x.PhoneNumber == viewModel.PhoneNumber
                                            || x.NormalizedUserName == viewModel.UserName.ToUpper()).FirstOrDefault();
            if (userExists != null)
            {
                return userExists.IsDeleted
                    ? $"{StatusName.Failed} User with {viewModel.UserName} already exists in inactive state!"
                    : $"{StatusName.Failed} User with {viewModel.UserName} already exists!";
            }

            var appuserID = _db.Users.OrderByDescending(x => x.AppUserId).Select(x => x.AppUserId).FirstOrDefault();
            appuserID = appuserID == 0 ? 1 : appuserID + 1;
            //Add User
            User user = new()
            {
                Id = Guid.NewGuid(),
                Email = viewModel.Email,
                NormalizedEmail = viewModel.Email.ToUpper(),
                UserName = viewModel.UserName,
                NormalizedUserName = viewModel.UserName.ToUpper(),
                PhoneNumber = viewModel.PhoneNumber,
                PhoneNumberConfirmed = false,
                AppUserId = appuserID,
                EmailConfirmed = false,
                PasswordHash = CreateHashPassword(viewModel.Password)
            };
            TrackUser.Created(user);
            _db.Users.Add(user);

            //Assign Role
            var roleID = _db.Roles.Where(x => x.Name == RoleName.HDUser).Select(x => x.Id).FirstOrDefault();
            UserRole userRole = new()
            {
                UserId = user.Id,
                RoleId = roleID,
            };
            TrackUser.Created(userRole);
            _db.UserRoles.Add(userRole);
            _db.SaveChanges();
            return StatusName.Success;
        }
        public string Login(LoginViewModel viewModel)
        {
            string response = CheckUsenamePassword(viewModel);
            if (response == StatusName.Success)
            {
                string token = GenerateToken(viewModel.UserName);
                return (StatusName.Success + token);
            }
            return response;
        }
        public string ChangePassword(LoginViewModel viewModel)
        {
            if (viewModel.NewPassword is null) return "Please enter new password correctly!";

            string response = CheckUsenamePassword(viewModel);
            if (response == StatusName.Success)
            {
                string normalizedValue = viewModel.UserName.ToUpper();
                var userExists = _db.Users.Where(x => x.NormalizedEmail == normalizedValue || x.PhoneNumber == normalizedValue
                                            || x.NormalizedUserName == normalizedValue).FirstOrDefault();
                if (userExists is not null)
                {
                    string newPass = CreateHashPassword(viewModel.NewPassword);
                    userExists.PasswordHash = newPass;
                    _db.SaveChanges();
                    return StatusName.Success;
                }
                return $"User not found with {viewModel.UserName}";
            }
            return response;
        }
        #endregion

        #region RoleAndUserRole
        public string AddUpdRole(RoleViewModel viewModel)
        {
            if (viewModel.Id == null || viewModel.Id == Guid.Empty)
            {
                var roleExist = _db.Roles.FirstOrDefault(x => x.Name.ToLower() == viewModel.Name.ToLower());
                if (roleExist != null)
                {
                    return roleExist.IsDeleted
                        ? $"{StatusName.Failed} Role with {viewModel.Name} already exists in inactive state!"
                        : $"{StatusName.Failed} Role with {viewModel.Name} already exists!";
                }
                var newRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = viewModel.Name,
                    IsDeleted = false,
                };
                TrackUser.Created(newRole);
                _db.Roles.Add(newRole);
                _db.SaveChanges();
                return "Role added successfully.";
            }
            else
            {
                var role = _db.Roles.FirstOrDefault(x => x.Id == viewModel.Id);
                if (role == null) return $"{StatusName.Failed} Role not found!";
                role.Name = viewModel.Name;
                TrackUser.Updated(role);
                _db.SaveChanges();
                return "Role updated successfully.";
            }
        }
        public string ActiveInactiveRole(bool flag, Guid id)
        {
            var role = _db.Roles.FirstOrDefault(x => x.Id == id);
            if (role == null) return $"{StatusName.Failed}Role not found!";
            role.IsDeleted = flag;
            _db.SaveChanges();
            return StatusName.Success;
        }
        public List<Role> GetRoles()
        {
            return [.. _db.Roles];
        }

        #endregion
    }
}
