namespace TaskForATON.ViewModels;

public struct AuthUser
{
    public required Guid Guid { get; set; }
    public required string Login { get; set; }
    public required bool IsAdmin { get; set; }
}