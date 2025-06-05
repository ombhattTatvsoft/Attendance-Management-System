using Entity.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.IService;

public interface IHomeService
{
    Task<User?> AuthenticateUser(string email,string password);
    string CreateJwtToken(User userfound);
    void CreateCookie(User userfound, HttpContext httpContext, string token);
}
