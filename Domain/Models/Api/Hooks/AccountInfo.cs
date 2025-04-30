namespace Domain.Models.Api.Hooks;

public class AccountInfo
{
    public ulong PostBalance { get; set; }
    public ulong PreBalance { get; set; }
    public string Pubkey { get; set; }
}