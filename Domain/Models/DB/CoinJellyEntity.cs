using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;


[Table("coin_jelly")]
public class CoinJellyEntity
{
  [Key] [Column("id")] public Guid Id { get; set; } = Guid.NewGuid();
  [Column("company_wallet_address")] public string CompanyWalletAddress { get; set; }
  [Column("crypto_currency")] public string CryptoCurrency { get; set; }
  [Column("crypto_currency_chain")] public string CryptoCurrencyChain { get; set; }

}