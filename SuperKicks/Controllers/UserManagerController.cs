using Microsoft.AspNetCore.Mvc;
using SuperKicks.Repo;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagerController(IUserManagerRepository userRepository) : ControllerBase
    {
        private readonly IUserManagerRepository _userRepository = userRepository;

        #region UserAndLogin
        [HttpGet(@"validateUser/{validateBy}/{value}")]
        public IActionResult ValidateUser(string validateBy, string value)
        {
            var result = _userRepository.ValidateCredential(validateBy, value);
            return result == "Success" ? Ok(result) : BadRequest(result);
        }
        [HttpPost(@"createUser")]
        public IActionResult CreateUser([FromBody] UserViewModel viewModel)
        {
            bool result = _userRepository.CreateUser(viewModel);
            return result ? Ok($"user with {viewModel.UserName} is created successfully.")
                        : Unauthorized($"user with {viewModel.UserName} email is alreay exists!");
        }
        [HttpPost(@"login")]
        public IActionResult Login([FromBody] LoginViewModel viewModel)
        {
            string result = _userRepository.Login(viewModel);
            return result.StartsWith(StatusName.Success) ? Ok($"Login successfull \n{result.Replace(StatusName.Success, "")}") : Unauthorized(result);
        }
        [HttpPost(@"ChangePassword")]
        public IActionResult ChangePassword([FromBody] LoginViewModel viewModel)
        {
            string result = _userRepository.ChangePassword(viewModel);
            return result == StatusName.Success ? Ok("Password changed successfully.") : BadRequest("Please Enter correct password!");
        }
        #endregion
        /*
        #region Role
        [HttpPost(@"addRole")]
        public IActionResult AddRole([FromBody] RoleViewModel viewModel)
        {

        }
        #endregion
        */
    }
}