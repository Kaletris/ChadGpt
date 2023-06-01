using ChatGpt.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatGpt.Controllers;

[ApiController]
[Route("messages/{messageId:int}")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessagesController : ControllerBase
{
    private readonly MessagingContext context;

    public MessagesController(MessagingContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpPut]
    public ActionResult EditMessage(int messageId, [FromBody] string text)
    {
        var message = context.Messages.Find(messageId);
        if (message == null) return NotFound();
        if (User.Identity!.Name != message.UserId) return Forbid();
        message.Text = text;
        message.Edited = true;
        context.SaveChanges();
        return Ok();
    }


    [HttpDelete]
    public ActionResult DeleteMessage(int messageId)
    {
        var message = context.Messages.Find(messageId);
        if (message == null) return NotFound();
        if (User.Identity!.Name != message.UserId) return Forbid();

        context.Messages.Remove(message);
        context.SaveChanges();
        return Ok();
    }

    [HttpPut("react")]
    public ActionResult React(int messageId, ReactionType type)
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
            context.SaveChanges();
            return Ok();
        }

        reaction.Type = type;

        context.SaveChanges();
        return Ok();
    }

    [HttpDelete("react")]
    public ActionResult DeleteReaction(int messageId)
    {
        var message = context.Messages.Include(message => message.Reactions)
            .SingleOrDefault(message => message.Id == messageId);
        if (message == null) return NotFound();
        var reaction = message.Reactions!.Find(reaction => User.Identity!.Name == reaction.UserId);
        if (reaction == null) return BadRequest("There was no reaction");
        context.Remove(reaction);
        context.SaveChanges();
        return Ok();
    }
}