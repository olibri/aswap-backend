namespace Domain.Enums;

public enum EscrowStatus
{
    PendingOnChain,
    OnChain,

    PartiallyOnChain,
    Singing,
    SignedByOneSide,

    Released,
    Cancelled
}