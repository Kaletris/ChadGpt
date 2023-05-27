using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatGpt.Data;

public class MessagingContext : IdentityDbContext
{
    public MessagingContext(DbContextOptions<MessagingContext> options) : base(options)
    {
        Threads = Set<Thread>();
        Messages = Set<Message>();
        Reactions = Set<Reaction>();
    }


    public DbSet<Thread> Threads { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<Reaction> Reactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Reaction>().HasKey(reaction => new { reaction.UserId, reaction.MessageId });
    }
}