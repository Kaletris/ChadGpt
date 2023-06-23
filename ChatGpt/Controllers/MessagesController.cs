using ChatGpt.Data;
using ChatGpt.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatGpt.Controllers;

[ApiController]
[Route("messages/{messageId:int}")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessagesController : ControllerBase
{
    private readonly MessagingContext context;
    private readonly IHubContext<NotificationHub> hubContext;

    public MessagesController(MessagingContext context, IHubContext<NotificationHub> hubContext)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.hubContext = hubContext;
    }

    [HttpPut]
    public async Task<ActionResult> EditMessage(int messageId, [FromBody] string text)
    {
        var message = context.Messages.Find(messageId);
        if (message == null) return NotFound();
        if (User.Identity!.Name != message.UserId) return Forbid();
        message.Text = text;
        message.Edited = true;
        context.Messages.Remove(message);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("MessageCreated", message.ThreadId);
        return Ok();
    }


    [HttpDelete]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var message = context.Messages.Find(messageId);
        if (message == null) return NotFound();
        if (User.Identity!.Name != message.UserId) return Forbid();

        context.Messages.Remove(message);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ThreadDeleted", message.ThreadId);
        return Ok();
    }

    [HttpPut("react")]
    public async Task<ActionResult> React(int messageId, [FromBody] ReactionType type)
    {
        var message = context.Messages.Include(message => message.Reactions)
            .SingleOrDefault(message => message.Id == messageId);
        if (message == null) return NotFound();
        var reaction = message.Reactions!.Find(reaction => User.Identity!.Name == reaction.UserId);
        if (reaction == null)
        {
            message.Reactions.Add(new Reaction
            {
                UserId = User.Identity!.Name!,
                Type = type
            });
            await context.SaveChangesAsync();

            await hubContext.Clients.All.SendAsync("React", message.ThreadId);
            return Ok();
        }

        reaction.Type = type;

        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("ThreadCreated");

        return Ok();
    }

    [HttpDelete("react")]
    public async Task<ActionResult> DeleteReaction(int messageId)
    {
        var message = context.Messages.Include(message => message.Reactions)
            .SingleOrDefault(message => message.Id == messageId);
        if (message == null) return NotFound();
        var reaction = message.Reactions!.Find(reaction => User.Identity!.Name == reaction.UserId);
        if (reaction == null) return BadRequest("There was no reaction");
        context.Remove(reaction);
        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("React", message.ThreadId);
        return Ok();
    }
}