using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanNGo_Backend.Dto;
using PlanNGo_Backend.Service;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;


        public AuthController(ApplicationDbContext context, AuthService authService)
        {
            _authService = authService;
            _context = context;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            Console.WriteLine("hi there");
            var authResponse = await _authService.Login(dto);
            if (authResponse == null)
            {
                return Unauthorized("Invalid email or password");
            }
            return Ok(authResponse);

        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            Console.WriteLine("hellooww ");
            var result = await _authService.Register(dto);
            if (result)
            {
                return BadRequest("User with this email already exists.");
            }
            return Ok("Registration successful.");
        }



        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            AuthResponseDto authResponseDto = await _authService.GoogleLogin(dto);

            return Ok(authResponseDto);
        }


    }

}

