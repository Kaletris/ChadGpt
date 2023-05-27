using Microsoft.AspNetCore.Identity;

namespace ChatGpt.Data;

public class Thread
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Message>? Messages { get; set; }

    public required string OwnerId { get; set; }
    public IdentityUser? Owner { get; init; }
}