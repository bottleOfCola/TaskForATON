using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TaskForATON.DbStuff;
using TaskForATON.DbStuff.Models;
using TaskForATON.ViewModels;

namespace TaskForATON.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController(UserDbContext userDbContext) : ControllerBase
{
    #region Create
    [HttpPost]
    [Authorize(Roles = AuthData.AdminRole)]
    public IActionResult Create(CreateUserModel createUserData)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var admin = JsonSerializer.Deserialize<AuthUser>(User.FindFirst(nameof(AuthUser))!.Value)!;
        DateTime now = DateTime.Now;
        UserModel newUser = new()
        {
            Guid = Guid.NewGuid(),
            Login = createUserData.Login,
            Password = createUserData.Password,
            Name = createUserData.Name,
            Gender = createUserData.Gender,
            Birthday = createUserData.Birthday,
            Admin = createUserData.Admin,
            CreatedOn = now,
            CreatedBy = admin.Login,
            ModifiedOn = now,
            ModifiedBy = admin.Login,
        };
        userDbContext.Users.Add(newUser);
        userDbContext.SaveChanges();
        return Ok(newUser.Guid.ToString());
    }
    #endregion
    #region Update
    [HttpPost]
    [Authorize]
    public void ChangeName(ChangeNameModel m) => ChangeProperty(m.Guid, x => x.Name = m.Name);

    [HttpPost]
    [Authorize]
    public void ChangeGender(ChangeGenderModel m) => ChangeProperty(m.Guid, x => x.Gender = m.Gender);

    [HttpPost]
    [Authorize]
    public void ChangeBirthday(ChangeBirthdayModel m) => ChangeProperty(m.Guid, x => x.Birthday = m.Birthday);

    [HttpPost]
    [Authorize]
    public void ChangePassword(ChangePasswordModel m) => ChangeProperty(m.Guid, x => x.Password = m.Password);

    [HttpPost]
    [Authorize]
    public void ChangeLogin(ChangeLoginModel m) => ChangeProperty(m.Guid, x => x.Login = m.Login);

    [NonAction]
    private void ChangeProperty<T>(Guid userId, Func<UserModel, T> expression)
    {
        if (User.FindFirst(nameof(AuthUser)) is not Claim claim) return;
        var curUser = JsonSerializer.Deserialize<AuthUser>(claim.Value)!;
        if (!curUser.IsAdmin && curUser.Guid != userId) return;
        DateTime now = DateTime.Now;
        UserModel usr = userDbContext.Users.FirstOrDefault(x => x.Guid == userId)!;
        expression(usr);
        userDbContext.SaveChanges();
    }
    #endregion
    #region Read
    [HttpGet]
    public Guid[] GetAllActiveUsers() => userDbContext.Users.Where(x => !x.RevokedOn.HasValue).OrderBy(x => x.CreatedOn).Select(x => x.Guid).ToArray();
    [HttpPost]
    [Authorize(Roles = AuthData.AdminRole)]
    public GetUserViewModel? GetUserWithLogin(StringModel m) => userDbContext.Users.Where(x => x.Login == m.Str).Select(x => new GetUserViewModel
    {
        Name = x.Name,
        Birthday = x.Birthday,
        Gender = x.Gender,
        RevokedOn = x.RevokedOn
    }).FirstOrDefault();

    [HttpPost]
    [Authorize]
    public IActionResult GetUserWithLoginAndPassword(LoginModel m)
    {
        var usr = JsonSerializer.Deserialize<AuthUser>(User.FindFirst(nameof(AuthUser))!.Value)!;
        if (usr.Login != m.Login) return Forbid();
        return Ok(userDbContext.Users.FirstOrDefault(x => x.Login == m.Login && x.Password == m.Password));
    }
    [HttpPost]
    public Guid[] GetAllUsersOlderThan(IntModel m)
    {
        DateTime minDate = DateTime.Now.AddYears(m.Number * -1);
        return userDbContext.Users.Where(x => x.Birthday.HasValue && x.Birthday.Value <= minDate).Select(x => x.Guid).ToArray();
    }
    #endregion
    #region Delete
    [HttpPost]
    [Authorize(Roles = AuthData.AdminRole)]
    public void DangerDelete(UserIdModel m)
    {
        var usr = userDbContext.Users.FirstOrDefault(x => x.Guid == m.Guid);
        userDbContext.Users.Remove(usr);
    }
    [HttpPost]
    [Authorize(Roles = AuthData.AdminRole)]
    public void SafeDelete(UserIdModel m)
    {
        if (User.FindFirst(nameof(AuthUser)) is not Claim claim) return;
        var admin = JsonSerializer.Deserialize<AuthUser>(claim.Value)!;
        DateTime now = DateTime.Now;
        UserModel usr = userDbContext.Users.FirstOrDefault(x => x.Guid == m.Guid)!;
        usr.RevokedOn = now;
        usr.RevokedBy = admin.Login;
        usr.ModifiedOn = now;
        usr.ModifiedBy = admin.Login;
        userDbContext.SaveChanges();
    }
    #endregion
}

public record ChangeNameModel(Guid Guid, string Name);
public record ChangeGenderModel(Guid Guid, int Gender);
public record ChangeBirthdayModel(Guid Guid, DateTime Birthday);
public record ChangeLoginModel(Guid Guid, string Login);
public record ChangePasswordModel(Guid Guid, string Password);
public record UserIdModel(Guid Guid);
public record IntModel(int Number);
public record StringModel(string Str);