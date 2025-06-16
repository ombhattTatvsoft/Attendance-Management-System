using Entity.Models;
using Entity.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BLL.IService;

public interface IHomeService
{
    Task<User?> AuthenticateUserAsync(string email,string password);
    string? CreateJwtToken(User user);
    void CreateCookie(User user, HttpContext httpContext, string token);
    string? PrepareLink(User userFound, HttpContext httpContext,bool? firstLogin=false);
    Response SendEmail(string email, string body, string subject);
    Task<Response> CheckResetLinkAsync(string id);
    Task<User?> FindUserByResetCodeAsync(string resetCode);
}
