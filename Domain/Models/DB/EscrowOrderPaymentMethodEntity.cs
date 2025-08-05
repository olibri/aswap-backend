using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("escrow_order_payment_methods")]
public sealed class EscrowOrderPaymentMethodEntity
{
  [Column("order_id")] public Guid OrderId { get; set; }
  public EscrowOrderEntity Order { get; set; } = default!;

  [Column("method_id")] public short MethodId { get; set; }
  public PaymentMethod.PaymentMethod Method { get; set; } = default!;
}