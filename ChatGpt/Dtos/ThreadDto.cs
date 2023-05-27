using Microsoft.AspNetCore.Identity;

namespace ChatGpt.Dtos;

public class ThreadDto
{
    public int Id { get; set; }
    public required IdentityUser? Owner { get; set; }
}