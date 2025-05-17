using System.ComponentModel.DataAnnotations;

namespace TaskForATON.ViewModels;

public class CreateUserModel
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    [Range(1, 3, ErrorMessage = "Gender can be only 1,2 or 3")]
    public required int Gender { get; set; }

    public required DateTime? Birthday { get; set; }
    public required bool Admin { get; set; }
}