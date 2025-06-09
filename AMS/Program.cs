using System.Text;
using BLL;
using BLL.IService;
using BLL.Service;
using DAL.IRepository;
using DAL.Repository;
using Entity.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// get connection string
string conn = builder.Configuration.GetConnectionString("Default")!;

// Add services to the container.
builder.Services.AddDbContext<AMSContext>(q => q.UseNpgsql(conn));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

// add automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.RequireHttpsMetadata = false;
       options.SaveToken = true;
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = builder.Configuration["Jwt:Issuer"],
           ValidAudience = builder.Configuration["Jwt:Audience"],
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
           ClockSkew = TimeSpan.Zero
       };
       options.Events = new JwtBearerEvents
       {
           OnChallenge = context =>
           {
               if (!context.Handled)
               {
                   context.HandleResponse();
                   context.Response.Cookies.Delete("JwtAMSToken");
                   context.Response.Redirect("/Home");
               }
               return Task.CompletedTask;
           },
           OnMessageReceived = context =>
           {
               if (context.Request.Cookies.ContainsKey("JwtAMSToken"))
               {
                   context.Token = context.Request.Cookies["JwtAMSToken"];
               }
               return Task.CompletedTask;
           }
       };
   });
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
