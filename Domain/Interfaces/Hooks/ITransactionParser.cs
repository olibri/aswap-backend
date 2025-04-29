namespace Domain.Interfaces.Hooks;

public interface ITransactionParser
{
    ParsedEvent? ParseTransaction(string json);
}