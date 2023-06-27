using System.Security.Claims;
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

    /// <summary>
    ///     Returns the List of Threads.
    /// </summary>
    /// <response code="200">Thread Created</response>
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

    /// <summary>
    ///     Creates a new Thread.
    /// </summary>
    /// <response code="200">Thread Created</response>
    [HttpPost]
    public async Task<ActionResult> CreateThread([FromBody] string name)
    {
        var thread = new Thread
        {
            Name = name,
            OwnerId = User.Identity!.Name!
        };

        context.Add(thread);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ThreadCreated");

        return Ok();
    }

    /// <summary>
    ///     Deletes a specific Thread.
    /// </summary>
    /// <response code="404">There is no such Thread</response>
    /// <response code="403">User has no right to Delete this Thread</response>
    /// <response code="200">Thread Deleted</response>
    [HttpDelete("{threadId:int}")]
    [Authorize("Admin")]
    public async Task<ActionResult> DeleteThread(int threadId)
    {
        var thread = await context.Threads.FindAsync(threadId);

        if (thread == null) return NotFound();

        var isAdmin = User.Claims.Any(claim => claim is { Type: ClaimTypes.Role, Value: "admin" });
        var isSender = User.Identity!.Name == thread.OwnerId;

        if (!isSender && !isAdmin) return Forbid();

        context.Threads.Remove(thread);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ThreadDeleted");

        return Ok();
    }

    /// <summary>
    ///     Returns the messages of a specific Thread.
    /// </summary>
    /// <response code="404">There is no such Thread</response>
    /// <response code="200">Returns the List of Messages</response>
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

    /// <summary>
    ///     Posts a Message to a specific Thread.
    /// </summary>
    /// <response code="404">There is no such Thread</response>
    /// <response code="400">There is no such User</response>
    /// <response code="200">Message Posted</response>
    [HttpPost("{threadId:int}/messages")]
    public async Task<ActionResult> CreateMessage(int threadId, [FromBody] string text)
    {
        var thread = await context.Threads.FindAsync(threadId);

        if (thread == null) return NotFound();

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