using BCrypt.Net;
using Google.Apis.Auth;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using PlanNGo_Backend.Dto;
using PlanNGo_Backend.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Service
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private static readonly HashSet<string> AllowedRoles = new() { "client", "organizer" };
        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> Register(RegisterDto dto)
        {
            Console.WriteLine(dto.Role);
            var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
            {
                return false;
            }
            var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password, 13);
            Console.WriteLine("Himanshu");
            User user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                IsEmailVerified = false,
                HashedPassword = hashedPassword,
                Role = dto.Role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            if (user.Role == "client")
            {
                Client client = new Client
                {
                    UserId = user.UserId,
                    User = user
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }
            else if (user.Role == "organizer")
            {
                Organizer organizer = new Organizer
                {
                    UserId = user.UserId,
                    User = user
                };
                _context.Organizers.Add(organizer);
                await _context.SaveChangesAsync();
            }



            return true;
        }


        public async Task<AuthResponseDto?> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == dto.Email.ToLower()
            );

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.HashedPassword))
                return null;

            return new AuthResponseDto
            {
                User = user,
                Token = GenerateJwtToken(user),
                ExpiresIn = _configuration.GetValue<int>("JwtConfig:ExpiryInMinutes") * 60
            };
        }


        public async Task<AuthResponseDto> GoogleLogin(GoogleLoginDto dto)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                dto.IdToken,   // ✅ ID TOKEN
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                });

            var googleId = payload.Subject;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user == null)
            {
                user = new User
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    GoogleId = googleId,
                    IsEmailVerified = payload.EmailVerified,
                    Role = dto.Role ?? "client"
                };
                user.Role=user.Role.ToLower();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return new AuthResponseDto
            {
                User = user,
                Token = GenerateJwtToken(user),
                ExpiresIn = _configuration.GetValue<int>("Jwt:TokenValidityInMinutes") * 60
            };
        }



        public string GenerateJwtToken(User user)
        {
            var key = _configuration["JwtConfig:Secret"];
            var issuer = _configuration["JwtConfig:Issuer"];
            var audience = _configuration["JwtConfig:Audience"];
            var expiryMinutes = _configuration.GetValue<int>("JwtConfig:ExpiryInMinutes");

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("JWT Secret is missing in configuration");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
