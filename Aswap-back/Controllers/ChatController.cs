using Domain.Interfaces.Chat;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatDbCommand chatHandler, ILogger<ChatController> log) : Controller
{
    [HttpGet]
    [Route("rooms/{roomId}/messages")]
    public async Task<IActionResult> GetMessages(ulong roomId)
    {
        log.LogInformation("Update order request");

        var messages = await chatHandler.GetMessagesAsync(roomId);
        return Ok(messages);
    }


    [HttpPost]
    [Route("add-messages")]
    public async Task<IActionResult> PostMessage([FromBody] MessageDto message)
    {
        log.LogInformation("Update order request");

        await chatHandler.CreateMessageAsync(message);
        return Ok();
    }
}