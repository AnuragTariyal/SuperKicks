using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult CreateUser([FromBody] UserViewModel vModel)
        {
            bool result = _userRepository.CreateUser(vModel);
            return result ? Ok($"user with {vModel.UserName} is created successfully.")
                        : Unauthorized($"user with {vModel.UserName} email is alreay exists!");
        }
        [HttpPost(@"login")]
        public IActionResult Login(LoginViewModel vmModel)
        {
            bool result = _userRepository.Login(vmModel);
            return result ? Ok("Login successfull") : Unauthorized("Invalid username or password!");
        }
    }
}