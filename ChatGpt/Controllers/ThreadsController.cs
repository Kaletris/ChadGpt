using ChatGpt.Data;
using ChatGpt.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Thread = ChatGpt.Data.Thread;

namespace ChatGpt.Controllers;

[ApiController]
[Route("threads")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ThreadsController : ControllerBase
{
    private readonly MessagingContext context;

    private readonly UserManager<IdentityUser> userManager;

    public ThreadsController(MessagingContext context, UserManager<IdentityUser> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    [HttpGet]
    public List<Thread> GetThreads()
    {
        return context.Threads.ToList();
    }

    [HttpPost]
    public void CreateThread(string name)
    {
        var thread = new Thread
        {
            Name = name,
            OwnerId = User.Identity!.Name!
        };

        context.Add(thread);
        context.SaveChanges();
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

        return Ok();
    }
}