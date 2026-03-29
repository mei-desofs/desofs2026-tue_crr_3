using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserRepository userRepository) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
        {
            var users = await _userRepository.GetAllActive();
            return Ok(users);
        }
    }
}
