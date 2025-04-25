using Domain.Interfaces.Database.Command;
using Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("[controller]")]
public class MarketController(IMarketDbCommand dbCommand) : ControllerBase
{
    [Route("create-order")]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrderAsync(CreateOrderDto orderDto)
    {
        var res =  await dbCommand.CreateOrderAsync(orderDto);
        return Ok(res);
    }
}