namespace Domain.Models.Api.Auth;

public sealed record WalletAuthDto(string Network, string Wallet, string Nonce, string Signature);