namespace TaskForATON.DbStuff.Models;

public class UserModel
{
    public Guid Guid { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required int Gender { get; set; }

    public DateTime? Birthday { get; set; }
    public required bool Admin { get; set; }

    public required DateTime CreatedOn { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime ModifiedOn { get; set; }
    public required string ModifiedBy { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}