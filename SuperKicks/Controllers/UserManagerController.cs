using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperKicks.Repo;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserManagerController(IUserManagerRepository userManagerRepository) : ControllerBase
    {
        private readonly IUserManagerRepository _userManagerRepository = userManagerRepository;

        #region UserAndLogin
        [HttpGet(@"getUsers")]
        public IActionResult GetUsers()
        {
            return Ok(_userManagerRepository.GetUsers());
        }
        [AllowAnonymous]
        [HttpGet(@"validateUser/{validateBy}/{value}")]
        public IActionResult ValidateUser(string validateBy, string value)
        {
            var result = _userManagerRepository.ValidateCredential(validateBy, value);
            return result == "Success" ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost(@"createUser")]
        public IActionResult CreateUser([FromBody] UserViewModel viewModel)
        {
            string result = _userManagerRepository.CreateUser(viewModel);
            return result == StatusName.Success
                        ? Ok($"user with {viewModel.UserName} is created successfully.")
                        : BadRequest($"user with {viewModel.UserName} email is alreay exists!");
        }

        [AllowAnonymous]
        [HttpPost(@"login")]
        public IActionResult Login([FromBody] LoginViewModel viewModel)
        {
            string result = _userManagerRepository.Login(viewModel);
            return result.StartsWith(StatusName.Success)
                ? Ok($"Login successfull \n{result.Replace(StatusName.Success, "")}")
                : BadRequest(result);
        }

        [HttpPost(@"changePassword")]
        public IActionResult ChangePassword([FromBody] LoginViewModel viewModel)
        {
            string result = _userManagerRepository.ChangePassword(viewModel);
            return result == StatusName.Success ? Ok("Password changed successfully.") : BadRequest("Please Enter correct password!");
        }
        #endregion

        #region RoleAndUserRole
        [HttpGet(@"getRoles")]
        public IActionResult GetRoles()
        {
            return Ok(_userManagerRepository.GetRoles());
        }
        [HttpPost(@"addUpdRole")]
        public IActionResult AddUpdRole([FromBody] RoleViewModel viewModel)
        {
            var result = _userManagerRepository.AddUpdRole(viewModel);
            return result.StartsWith(StatusName.Failed) ? BadRequest(result.Replace(StatusName.Failed, "")) : Ok(result);
        }
        [HttpPost(@"activeInactiveRole/{flag:bool}/{id:guid}")]
        public IActionResult ActiveInactive(bool flag, Guid id)
        {
            var result = _userManagerRepository.ActiveInactiveRole(flag, id);
            return result == StatusName.Success
                            ? (Ok(flag ? "Role is activate" : "Role is inactivate"))
                            : BadRequest(result.Replace(StatusName.Failed, ""));
        }
        #endregion
    }
}