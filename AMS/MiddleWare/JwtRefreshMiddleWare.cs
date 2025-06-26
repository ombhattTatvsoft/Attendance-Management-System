
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BLL.IService;
using Entity.Models;

namespace AMS.MiddleWare
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext context,IServiceProvider serviceProvider)
        {
            var jwtCookie = context.Request.Cookies["JwtAMSToken"];

            if (jwtCookie!=null)
            {
                var handler = new JwtSecurityTokenHandler();

                try
                {
                    var token = handler.ReadJwtToken(jwtCookie);
                    var exp = token.ValidTo;
                    if (DateTime.UtcNow > exp)
                    {
                        var _service=serviceProvider.GetRequiredService<IUserService>();
                        var _authService=serviceProvider.GetRequiredService<IAuthService>();
                        int userId = int.Parse(token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                        User? user= await _service.GetUserById(userId);
                        if (user!=null && user.RememberTime>=DateTime.Now)
                        {
                            string? newToken = _authService.CreateJwtToken(user);
                            _authService.CreateCookie(user,context,newToken!,true,true);
                            context.Request.Headers["Authorization"] = $"Bearer {newToken}";
                        }
                    }
                    else{
                        context.Request.Headers["Authorization"] = $"Bearer {jwtCookie}";
                    }
                }
                catch
                {
                }
            }

            await _next(context);
        }
    }

}

