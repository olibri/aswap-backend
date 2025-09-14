namespace Domain.Models.Api.QuerySpecs;

public record PageSpec(int Number = 1, int Size = 20)
{
  public int Skip => (Number - 1) * Size;
  public int Take => Size;
}