using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Service;

public class HomeService : IHomeService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    public HomeService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }
    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        User? user = await _userRepository.GetAsync(x => x.Email.ToLower() == email.ToLower(),x=>x.Role);
        if (user != null)
        {
            if (BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            {
                return user;
            }
        }
        return null;
    }
    public string CreateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Role, user.Role.RoleName)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public void CreateCookie(User user, HttpContext httpContext, string token)
    {
        CookieOptions options = new()
        {
            Domain = "localhost", // Set the domain for the cookie
            Expires = DateTime.Now.AddDays(7), // Set expiration date to 7 days from now
            Path = "/", // Cookie is available within the entire application
            Secure = false, // Ensure the cookie is only sent over HTTPS
            HttpOnly = true, // Prevent client-side scripts from accessing the cookie
            IsEssential = true // Indicates the cookie is essential for the application to function
        };
        httpContext.Response.Cookies.Append("JwtAMSToken", token, options);
    }
}
