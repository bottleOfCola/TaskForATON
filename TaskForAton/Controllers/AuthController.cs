using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using TaskForATON.DbStuff;
using TaskForATON.DbStuff.Models;
using TaskForATON.ViewModels;

namespace TaskForATON.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(UserDbContext userDbContext) : ControllerBase
{
    [HttpPost]
    public IActionResult Login(LoginModel loginVM)
    {
        if (userDbContext.Users.FirstOrDefault(x => x.Login == loginVM.Login && x.Password == loginVM.Password) is not UserModel usr) return Forbid();
        if (usr.RevokedOn is not null) return Forbid();

        string userData = JsonSerializer.Serialize(new AuthUser
        {
            Guid = usr.Guid,
            Login = loginVM.Login,
            IsAdmin = usr.Admin
        });
        List<Claim> claims = [new(nameof(AuthUser), userData)];

        if (usr.Admin) claims.Add(new(ClaimsIdentity.DefaultRoleClaimType, AuthData.AdminRole));
        else claims.Add(new(ClaimsIdentity.DefaultRoleClaimType, AuthData.UserRole));

        ClaimsIdentity claimsIdentity = new(claims, "Bearer");
        ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
        // установка аутентификационных куки
        var jwt = new JwtSecurityToken(
            issuer: "MyAuthServer",
            audience: "MyAuthClient",
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)), // время действия 2 минуты
            signingCredentials: new SigningCredentials(AuthData.Key, SecurityAlgorithms.HmacSha256));

        return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
    }
}

public record LoginModel(string Login, string Password);