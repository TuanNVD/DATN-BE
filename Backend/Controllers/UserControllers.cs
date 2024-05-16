using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Context;
using Backend.Entities;
using Backend.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class UserControllers : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public UserControllers(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.email && u.Password == model.password);
            if (user == null)
                return BadRequest(new { message = "Username or password wrong" });
            var token = GenerateJwtToken(user);
            var response = new AuthenticateResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
            };
            return Ok(response);
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] RegisterRequest model)
        {
            if (_context.Users.Any(x => x.Email == model.Email))
                return BadRequest(new {message = "Email already exists" });
            if (model.ConfirmPassword != model.Password) return BadRequest(new { message = "confimpassword not right" });
            var User = new User { 
                Name = model.Name,
                Email = model.Email,
                Password = model.Password 
            };
            _context.Users.Add(User);
            _context.SaveChanges();
            return Ok(new { message = "Register successfull" });

        }
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:ValidIssuer"],
                audience: _config["Jwt:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: credentials);
            var encodetoken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }
    }
}
