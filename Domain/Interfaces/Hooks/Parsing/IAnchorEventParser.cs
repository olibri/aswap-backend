namespace Domain.Interfaces.Hooks.Parsing;

public interface IAnchorEventParser
{
    IEnumerable<object> Parse(string[] logs);
}