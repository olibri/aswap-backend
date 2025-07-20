using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Api;

public sealed record SessionPingDto(Guid SessionId, string Wallet);