namespace Domain.Interfaces.Hooks.Parsing;

public interface IAnchorEventParser
{
    IEnumerable<IAnchorEvent> Parse(string[] logs);
}