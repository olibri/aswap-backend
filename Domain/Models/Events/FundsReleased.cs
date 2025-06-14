﻿using Domain.Interfaces.Hooks.Parsing;
using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;

[BorshObject]
public class FundsReleased : IAnchorEvent
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Buyer { get; set; } = new byte[32];

    [BorshPropertyOrder(2)] public ulong Amount { get; set; }
    [BorshPropertyOrder(3)] public ulong DealId { get; set; }
    [BorshPropertyOrder(4)] public long Ts { get; set; }

    public override string ToString()
    {
        return $"FundsReleased {{ Escrow={ConvertHelper.ToBase58(Escrow)}, " +
               $"Buyer={ConvertHelper.ToBase58(Buyer)}, Amount={Amount}, " +
               $"DealId={DealId}, Ts={Ts} }}";
    }
}