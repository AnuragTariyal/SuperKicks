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

        [HttpPost(@"createuser")]
        public IActionResult CreateUser([FromBody] UserViewModel vModel)
        {
            var message = new
            {
                Message = _userRepository.CreateUser(vModel)
            };
            return Ok(message);
        }
    }
}