namespace Tests_back.Extensions;

public static class TestJson
{
    public const string OfferInitialized = @"
{
  ""data"": [
    {
      ""signature"": ""3Ei1jqB4JEBpYDjuci4wQBFEThJvGdeh8ohZmoE7PKxYMSWkHUo3NebgNzGv1mmdkqxDw1fQ9QCoJT9NRnur45hd"",
      ""slot"": 408858789,
      ""blockTime"": 1758270214,
      ""logs"": [
        ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
        ""Program ComputeBudget111111111111111111111111111111 success"",
        ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
        ""Program ComputeBudget111111111111111111111111111111 success"",
        ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL invoke [1]"",
        ""Program log: Create"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
        ""Program log: Instruction: GetAccountDataSize"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 1595 of 194255 compute units"",
        ""Program return: TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA pQAAAAAAAAA="",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
        ""Program 11111111111111111111111111111111 invoke [2]"",
        ""Program 11111111111111111111111111111111 success"",
        ""Program log: Initialize the associated token account"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
        ""Program log: Instruction: InitializeImmutableOwner"",
        ""Program log: Please upgrade to SPL Token 2022 for immutable owner support"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 1405 of 187642 compute units"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
        ""Program log: Instruction: InitializeAccount3"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4214 of 183758 compute units"",
        ""Program TokenkegQfeZyiNwAJbNbGKDtHtW success"",
        ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL consumed 20460 of 199700 compute units"",
        ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL success"",
        ""Program 7HhnrDB8Y71oH5vUAE863oqKJmVEXiA4k7Z8xLUDtHtW invoke [1]"",
        ""Program log: Instruction: InitializeOffer"",
        ""Program 11111111111111111111111111111111 invoke [2]"",
        ""Program 11111111111111111111111111111111 success"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
        ""Program log: Instruction: Transfer"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4645 of 165632 compute units"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
        ""Program log: Offer created by CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW, amount: 33000000"",
        ""Program data: TWIEKZ3vRy7b39oo9YvR1TUyOFnkWYcbaNjUZNxRdPdthOH6dUwCqq5RvUSDa6JicyPlgPOGxE74rdj1OLcLAwc1HsEkdzhPAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADpKDlVCWX/1NZKyq9G1F33MY5bT1fJDEh9YGJdgpuDe1VTRAAAAAAAQIr3AQAAAAAQJwAAAAAAAEM/EmGZAQAABhPNaAAAAAAA"",
        ""Program 7HhnrDB8Y71oH5vUAE863oqKJmVEXiA4k7Z8xLUDtHtW consumed 32250 of 179240 compute units"",
        ""Program 7HhnrDB8Y71oH5vUAE863oqKJmVEXiA4k7Z8xLUDtHtW success""
      ],
      ""success"": true,
      ""programInvocations"": [
        {
          ""programId"": ""ComputeBudget111111111111111111111111111111"",
          ""instruction"": {
            ""data"": ""3qi1MbrtkR83"",
            ""accounts"": []
          }
        },
        {
          ""programId"": ""ComputeBudget111111111111111111111111111111"",
          ""instruction"": {
            ""data"": ""Fj2Eoy"",
            ""accounts"": []
          }
        },
        {
          ""programId"": ""ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL"",
          ""instruction"": {
            ""parsed"": {
              ""info"": {
                ""account"": ""7pUEhKFchY8isz7NiHq8atGibd3FxksxxUM64jNr2Sc6"",
                ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""source"": ""CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW"",
                ""systemProgram"": ""11111111111111111111111111111111"",
                ""tokenProgram"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"",
                ""wallet"": ""AL9fpsh34nCQADJ7c5srqufGcAUm9Md6i6gvn7bFJ5KG""
              },
              ""type"": ""create""
            },
            ""program"": ""spl-associated-token-account"",
            ""programId"": ""ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL"",
            ""stackHeight"": 1,
            ""accounts"": []
          }
        },
        {
          ""programId"": ""7HhnrDB8Y71oH5vUAE863oqKJmVEXiA4k7Z8xLUDtHtW"",
          ""instruction"": {
            ""data"": ""cabmptwkPkhQTpDshU7PDNkGPYep2s1Pmvn7mkc1VQGgcfCgzrVHkRkY7CDF1ifD1r44n5oCCrHtJm8wT2CW5tEZfUv3MSk5ao"",
            ""accounts"": [
              {
                ""pubkey"": ""FoJGBm41fEtCCB6B4mXSexFE9jUr4pDgfxbqFpHSfrGZ"",
                ""preBalance"": 0,
                ""postBalance"": 2707440
              },
              {
                ""pubkey"": ""C1q1it2y7PBaVnDUcMrBkyxBeiBrYeQgQ6Q3hqdcM5BN"",
                ""preBalance"": 2039280,
                ""postBalance"": 2039280
              },
              {
                ""pubkey"": ""7pUEhKFchY8isz7NiHq8atGibd3FxksxxUM64jNr2Sc6"",
                ""preBalance"": 0,
                ""postBalance"": 2039280
              },
              {
                ""pubkey"": ""CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW"",
                ""preBalance"": 4983890080,
                ""postBalance"": 4979063360
              },
              {
                ""pubkey"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"",
                ""preBalance"": 5268187701,
                ""postBalance"": 5268187701
              },
              {
                ""pubkey"": ""11111111111111111111111111111111"",
                ""preBalance"": 1,
                ""postBalance"": 1
              }
            ]
          }
        }
      ]
    }
  ]
}
";
}