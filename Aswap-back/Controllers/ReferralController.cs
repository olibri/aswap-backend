using App.Utils;
using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Referral;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferralController(IReferralService referralService) : Controller
{
  [HttpPost("generate")]
  [Authorize]
  public async Task<ActionResult<GenerateReferralResponse>> GenerateReferralCode(CancellationToken ct)
  {
    var wallet = User.GetUserWallet();

    var code = await referralService.GenerateReferralCodeAsync(wallet, ct);
    var link = await referralService.GetReferralLinkAsync(wallet, ct);

    return Ok(new GenerateReferralResponse
    {
      ReferralCode = code,
      ReferralLink = link
    });
  }

  [HttpPost("process")]
  public async Task<ActionResult<bool>> ProcessReferral([FromBody] ProcessReferralRequest request, CancellationToken ct)
  {
    if (string.IsNullOrEmpty(request.ReferralCode) || string.IsNullOrEmpty(request.NewUserWallet))
    {
      return BadRequest("ReferralCode and NewUserWallet are required");
    }

    var success = await referralService.ProcessReferralAsync(request.ReferralCode, request.NewUserWallet, ct);

    if (!success)
    {
      return BadRequest("Invalid referral code or user already referred");
    }

    return Ok(true);
  }

  [HttpGet("stats")]
  [Authorize]
  public async Task<ActionResult<ReferralStatsDto>> GetReferralStats(CancellationToken ct)
  {
    var wallet = User.GetUserWallet();
    var stats = await referralService.GetReferralStatsAsync(wallet, ct);
    return Ok(stats);
  }

  [HttpGet("referred-users")]
  [Authorize]
  public async Task<ActionResult<List<ReferredUserDto>>> GetReferredUsers(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      CancellationToken ct = default)
  {
    var wallet = User.GetUserWallet();
    var users = await referralService.GetReferredUsersAsync(wallet, page, pageSize, ct);
    return Ok(users);
  }

  [HttpGet("validate/{referralCode}")]
  public async Task<ActionResult<bool>> ValidateReferralCode(string referralCode, CancellationToken ct)
  {
    var isValid = await referralService.ValidateReferralCodeAsync(referralCode, ct);
    return Ok(isValid);
  }

  [HttpGet("link")]
  [Authorize]
  public async Task<ActionResult<string>> GetReferralLink(CancellationToken ct)
  {
    var wallet = User.GetUserWallet();
    var link = await referralService.GetReferralLinkAsync(wallet, ct);
    return Ok(link);
  }
}