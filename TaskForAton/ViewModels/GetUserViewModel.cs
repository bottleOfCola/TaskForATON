namespace TaskForATON.ViewModels;

public class GetUserViewModel
{
    public required string Name { get; set; }
    public required int Gender { get; set; }

    public required DateTime? Birthday { get; set; }
    public required DateTime? RevokedOn { get; set; }
}