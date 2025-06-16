using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.IService;
using DAL.IRepository;
using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Service;

public class HomeService : IHomeService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly ILogger<HomeService> _logger;
    public HomeService(IUserRepository userRepository, IConfiguration config, ILogger<HomeService> logger)
    {
        _userRepository = userRepository;
        _config = config;
        _logger = logger;
    }
    public async Task<User?> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            User? user = await _userRepository.GetAsync(x => x.Email.ToLower() == email.ToLower(), x => x.Role);
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
                {
                    return user;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user with email: {Email}", email);
            return null;
        }
    }
    public string? CreateJwtToken(User user)
    {
        try
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating JWT token for user: {UserId}", user.UserId);
            return null;
        }

    }
    public void CreateCookie(User user, HttpContext httpContext, string token)
    {
        try
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating cookie for user: {UserId}", user.UserId);
        }
    }

    public string? PrepareLink(User userFound, HttpContext httpContext,bool? firstLogin=false)
    {
        try
        {
            string resetCode = Guid.NewGuid().ToString();
            string verifyUrl = "/Home/resetpassword/" + resetCode;
            string link = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{verifyUrl}";
            userFound.ResetPasswordCode = resetCode;
            if (firstLogin == true)
                userFound.FirstLogin = true;
            else
                userFound.ResetPasswordCodeExpiryTime = DateTime.Now.AddHours(24);
            _userRepository.Update(userFound);
            _userRepository.SaveAsync();
            return link;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email reset link preparation failed");
            return null;
        }
    }
    public Response SendEmail(string email, string body, string subject)
    {
        try
        {
            using MailMessage mm = new(_config["MailMessageCred:Email"]!, email);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = true;
            SmtpClient smtp = new()
            {
                Host = _config["MailMessageCred:Host"]!,
                EnableSsl = true
            };
            NetworkCredential NetworkCred = new(_config["MailMessageCred:Email"]!, _config["MailMessageCred:Password"]!);
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            smtp.Send(mm);
            return new Response(true, "Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sent failed for {}",subject);
            return new Response(false, "Email sent failed");
        }
    }

    public async Task<Response> CheckResetLinkAsync(string id)
    {
        User? user=await _userRepository.GetAsync(x=>x.ResetPasswordCode==id);
        if(user==null)
        return new Response(false,"Invalid Link");
        else{
            if(!user.FirstLogin && (user.ResetPasswordCodeExpiryTime==null || DateTime.Now>user.ResetPasswordCodeExpiryTime)){
                user.ResetPasswordCode=null;
                user.ResetPasswordCodeExpiryTime=null;
                _userRepository.Update(user); 
                return new Response(false,"Reset Link Expired.");
            }
            return new Response(true);
        }
    }

    public async Task<User?> FindUserByResetCodeAsync(string resetCode)
    {
        return await _userRepository.GetAsync(x => x.ResetPasswordCode == resetCode);
    }
}
