using Microsoft.AspNetCore.Identity;

namespace ChatGpt.Data;

public class Message
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public DateTime Time { get; set; }
    public required string UserId { get; set; }
    public IdentityUser? User { get; set; }

    public bool Edited { get; set; }

    public Thread? Thread { get; set; }
    public required int ThreadId { get; set; }

    public List<Reaction>? Reactions { get; set; }
}