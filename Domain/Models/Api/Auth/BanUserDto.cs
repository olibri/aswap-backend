namespace Domain.Models.Api.Auth;

public record BanUserDto(string Wallet, string Reason, DateTime? Until);