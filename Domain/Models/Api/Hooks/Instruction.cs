using System.Text.Json;

namespace Domain.Models.Api.Hooks;

public class Instruction
{
    public List<AccountInfo> Accounts { get; set; } = [];
    public JsonElement Data { get; set; }              
    public int Index { get; set; }
    public JsonElement TokenBalances { get; set; }     
}