using Microsoft.AspNetCore.Mvc;
using SuperKicks.Repo;
using SuperKicks.Repo.Repository.Interface;
using SuperKicks.Repo.ViewModels;

namespace SuperKicks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserRepository userRepository) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;

        [HttpGet(@"validateuser/{validateBy}/{value}")]
        public IActionResult ValidateUser(string validateBy, string value)
        {
            var result = _userRepository.ValidateCredential(validateBy, value);
            return result == "Success" ? Ok(result) : BadRequest(result);
        }
        [HttpPost(@"createuser")]
        public IActionResult CreateUser([FromBody] UserViewModel vmModel)
        {
            bool result = _userRepository.CreateUser(vmModel);
            return result ? Ok($"user with {vmModel.UserName} is created successfully.")
                        : Unauthorized($"user with {vmModel.UserName} email is alreay exists!");
        }
        [HttpPost(@"login")]
        public IActionResult Login(LoginViewModel vmModel)
        {
            string result = _userRepository.Login(vmModel);
            return result.StartsWith(StatusName.Success) ? Ok($"Login successfull \n{result.Replace(StatusName.Success, "")}") : Unauthorized(result);
        }
        [HttpPost(@"Changepassword")]
        public IActionResult ChangePassword(LoginViewModel vmModel)
        {
            string result = _userRepository.ChangePassword(vmModel);
            return result == StatusName.Success ? Ok("Password changed successfully.") : BadRequest("Please Enter correct password!");
        }
    }
}