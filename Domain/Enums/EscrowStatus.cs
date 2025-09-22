namespace Domain.Enums;

//public enum EscrowStatus
//{
//  PendingOnChain = 0,       // це для бай ордерів при першому створенні
//  OnChain = 1,              // це для сел ордерів при першому створенні

//  PartiallyOnChain = 2,     // це для сел/бай ордерів коли частково заповнено
//  Signed = 3,               // це для сел/бай ордерів коли обидві сторони підписали
//  SignedByOwnerSide = 4,      // це для сел/бай ордерів коли підписала одна сторона
//  SignedByContraAgentSide = 5,      // це для сел/бай ордерів коли підписала одна сторона

//  Released = 6,             // це для сел/бай ордерів коли релізнули кошти
//  Cancelled = 7,            // це для сел/бай ордерів коли скасували з селл(крипто сторони) і з бай(фіат сторони)
//  AdminResolving = 8        // це коли адмін розбирається з ордером
//}

public enum UniversalOrderStatus
{
  Created = 0,
  Active = 1,
  SignedByOneParty = 3,
  BothSigned = 4,
  Completed = 5,
  Cancelled = 6,
  AdminResolving = 7
}