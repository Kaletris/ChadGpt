using Microsoft.AspNetCore.Identity;

namespace ChatGpt.Data;

public class Reaction
{
    public required string UserId { get; set; }
    public IdentityUser? User { get; set; }
    public int MessageId { get; set; }
    public Message? Message { get; set; }
    public ReactionType Type { get; set; }
}