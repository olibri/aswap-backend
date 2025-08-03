using App.Services.Auth;
using App.Utils;
using Domain.Interfaces.Services;
using Domain.Interfaces.Services.Auth;
using Domain.Models.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
  ITokenService tokens,
  INonceService nonces,
  ISignatureVerifier verifier,
  IRefreshTokenService refreshStore,
  IClientIpAccessor ipAccessor,
  IAccountService accounts) : Controller
{
  [HttpPost]
  public async Task<IActionResult> Authenticate([FromBody] WalletAuthDto dto,
    CancellationToken ct)
  {
    var nonceOk = await nonces.ValidateAsync(dto.Wallet, dto.Nonce, ct);
    if (!nonceOk) return BadRequest("Invalid or expired nonce");

    if (!verifier.Verify("sol", dto.Wallet, dto.Nonce, dto.Signature))
      return BadRequest("Signature verification failed");

    var banUntil = await accounts.GetBanUntilAsync(dto.Wallet, ct);
    if (banUntil is not null && banUntil > DateTime.UtcNow)
      return StatusCode(403, "User is banned");

    var pair = tokens.Generate(dto.Wallet, "user", banUntil);

    if (!RefreshTokenService.TryParseRefresh(pair.RefreshToken, out var exp))
      return StatusCode(500, "Invalid refresh format");

    var ua = Request.Headers.UserAgent.ToString();
    var ip = ipAccessor.GetClientIp();
    await refreshStore.SaveAsync(dto.Wallet, pair.RefreshToken, ua, ip, exp, ct);

    return Ok(pair);
  }

  [HttpGet("nonce")]
  public IActionResult GetNonce([FromQuery] string wallet)
  {
    if (!AddressValidator.IsValidSolanaAddress(wallet))
      return BadRequest("Invalid wallet");

    var nonce = nonces.Issue(wallet);
    return Ok(new { nonce });
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
  {
    var session = await refreshStore.ValidateAsync(req.RefreshToken, ct);
    if (session is null) return Unauthorized("Invalid refresh");

    var (id, wallet) = session.Value;

    var banUntil = await accounts.GetBanUntilAsync(wallet, ct);
    if (banUntil is not null && banUntil > DateTime.UtcNow)
      return StatusCode(403, "User is banned");

    var pair = tokens.Generate(wallet, "user", banUntil);
    if (!RefreshTokenService.TryParseRefresh(pair.RefreshToken, out var newExp))
      return StatusCode(500, "Invalid refresh format");

    var ua = Request.Headers.UserAgent.ToString();
    var ip = ipAccessor.GetClientIp();
    await refreshStore.RotateAsync(id, req.RefreshToken, pair.RefreshToken, newExp, ua, ip, ct);

    return Ok(pair);
  }
}