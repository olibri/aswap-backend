using App.Utils;
using Domain.Interfaces.Services;
using Domain.Interfaces.Services.Auth;
using Domain.Models.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController(
  ITokenService tokens,
  INonceService nonces,
  ISignatureVerifier verifier,
  IAccountService accounts) : Controller
{
  [Authorize]
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

    var pair = tokens.Generate(dto.Wallet, role: "user", banUntil);

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
}

