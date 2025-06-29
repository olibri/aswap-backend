namespace Domain.Enums;

public enum EscrowStatus
{
    PendingOnChain,
    OnChain,

    PartiallyOnChain,
    Signed,
    SignedByOneSide,

    Released,
    Cancelled,
    AdminResolving
}