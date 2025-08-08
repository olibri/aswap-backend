namespace Domain.Enums;

public enum EscrowStatus
{
  PendingOnChain = 0,
  OnChain = 1,

  PartiallyOnChain = 2,
  Signed = 3,
  SignedByOneSide = 4,

  Released = 5,
  Cancelled = 6,
  AdminResolving = 7
}