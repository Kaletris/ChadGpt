using ChatGpt.Data;
using ChatGpt.Dtos;
using ChatGpt.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Thread = ChatGpt.Data.Thread;

namespace ChatGpt.Controllers;

[ApiController]
[Route("threads")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ThreadsController : ControllerBase
{
    private readonly MessagingContext context;
    private readonly IHubContext<NotificationHub> hubContext;

    private readonly UserManager<IdentityUser> userManager;

    public ThreadsController(MessagingContext context, UserManager<IdentityUser> userManager,
        IHubContext<NotificationHub> hubContext)
    {
        this.context = context;
        this.userManager = userManager;
        this.hubContext = hubContext;
    }

    [HttpGet]
    public List<ThreadDto> GetThreads()
    {
        return context.Threads.Select(thread => new ThreadDto
        {
            Id = thread.Id,
            Name = thread.Name,
            Owner = new UserDto
            {
                Id = thread.Owner!.Id,
                Name = thread.Owner!.UserName!
            }
        }).ToList();
    }

    [HttpPost]
    public async Task CreateThread([FromBody] string name)
    {
        var thread = new Thread
        {
            Name = name,
            OwnerId = User.Identity!.Name!
        };

        context.Add(thread);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ThreadCreated");
    }

    [HttpDelete("{threadId:int}")]
    [Authorize("Admin")]
    public async Task DeleteThread(int threadId)
    {
        var thread = await context.Threads.FindAsync(threadId);
        
        
    }

    [HttpGet("{threadId:int}/messages")]
    public List<MessageDto> GetMessages(int threadId)
    {
        return context.Messages.Where(message => message.ThreadId == threadId).Select(message => new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            Time = message.Time,
            Sender = new UserDto
            {
                Id = message.User!.Id,
                Name = message.User!.UserName!
            }
        }).ToList();
    }

    [HttpPost("{threadId:int}/messages")]
    public async Task<ActionResult> CreateMessage(int threadId, [FromBody] string text)
    {
        var user = await userManager.FindByIdAsync(User.Identity!.Name!);
        if (user == null) return BadRequest("No such user");

        var message = new Message
        {
            Text = text,
            Time = DateTime.Now,
            UserId = user.Id,
            ThreadId = threadId
        };

        context.Add(message);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("MessageCreated", threadId);

        return Ok();
    }
}