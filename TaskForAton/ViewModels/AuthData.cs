using Microsoft.IdentityModel.Tokens;

namespace TaskForATON.ViewModels;

public struct AuthData
{
    public static SymmetricSecurityKey Key { get; set; }
    public const string AdminRole = "admin";
    public const string UserRole = "user";
}