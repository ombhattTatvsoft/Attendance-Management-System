using Entity.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.IService;

public interface IHomeService
{
    Task<User?> AuthenticateUserAsync(string email,string password);
    string CreateJwtToken(User user);
    void CreateCookie(User user, HttpContext httpContext, string token);

}
