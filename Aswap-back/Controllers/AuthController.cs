using App.Services.Auth;
using App.Utils;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Services.Account;
using Domain.Interfaces.Services.Auth;
using Domain.Interfaces.Services.IP;
using Domain.Models.Api.Auth;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Aswap_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
  ITokenService tokens,
  INonceService nonces,
  ISignatureVerifier verifier,
  IRefreshTokenService refreshStore,
  IClientIpAccessor ipAccessor,
  IAccountService accounts,
  IAccountDbCommand accountDbCommand) : Controller
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

    var referralCode = Request.Cookies["referral_code"];
    await accounts.CreateAccountWithReferralAsync(dto.Wallet, referralCode, ct);

    if (!string.IsNullOrEmpty(referralCode))
    {
      Response.Cookies.Delete("referral_code");
    }


    var pair = tokens.Generate(dto.Wallet, "user", banUntil);

    if (!RefreshTokenService.TryParseRefresh(pair.RefreshToken, out var exp))
      return StatusCode(500, "Invalid refresh format");

    Response.Cookies.Append("refresh_token", pair.RefreshToken, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.Strict,
      Expires = exp
    });

    var ua = Request.Headers.UserAgent.ToString();
    var ip = ipAccessor.GetClientIp();

    await refreshStore.SaveAsync(new SaveRefreshDto
    (
      dto.Wallet,
      pair.RefreshToken,
      exp,
      ua,
      ip
    ), ct);

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
  public async Task<IActionResult> Refresh(CancellationToken ct)
  {
  
    var refresh = Request.Cookies["refresh_token"];
    if (string.IsNullOrEmpty(refresh)) return Unauthorized("No refresh cookie");

    var s = await refreshStore.ValidateAsync(refresh, ct);
    if (s is null) return Unauthorized("Invalid refresh");

    var (id, wallet) = s.Value;

    var banUntil = await accounts.GetBanUntilAsync(wallet, ct);
    if (banUntil is not null && banUntil > DateTime.UtcNow)
      return StatusCode(403, "User is banned");

    var pair = tokens.Generate(wallet, "user", banUntil);
    if (!RefreshTokenService.TryParseRefresh(pair.RefreshToken, out var newExp))
      return StatusCode(500, "Invalid refresh format");

    var ua = Request.Headers.UserAgent.ToString();
    var ip = ipAccessor.GetClientIp(); 

    await refreshStore.RotateAsync(
      new RotateRefreshDto(id, refresh, pair.RefreshToken, newExp, ua, ip),
      ct);

    Response.Cookies.Append("refresh_token", pair.RefreshToken, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.Strict,
      Expires = newExp,
      Path = "/"
    });

    return Ok(pair);
  }


  [HttpPost("logout")]
  public async Task<IActionResult> Logout(CancellationToken ct)
  {
    var refresh = Request.Cookies["refresh_token"];
    if (string.IsNullOrEmpty(refresh))
    {
      Response.Cookies.Delete("refresh_token");
      return Ok();
    }

    var s = await refreshStore.ValidateAsync(refresh, ct);
    if (s is not null) await refreshStore.RevokeAsync(s.Value.Id, ct);

    Response.Cookies.Delete("refresh_token");
    return Ok();
  }
}