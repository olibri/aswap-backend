namespace Tests_back.Extensions;

public static class TestJson
{
    public const string UniversalOrderCreated = @"
{
  ""data"": [
    {
      ""signature"": ""3bmuDyVJEZ9ynHf3r2KbE7S3YBeo28nrgKcSxmC1sncqYUSdxh8agApJ3SEaGEEp8HVGqpSQbcWXiLUKMCxNmLvH"",
      ""slot"": 409765085,
      ""blockTime"": 1758618002,
      ""logs"": [
        ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
        ""Program ComputeBudget111111111111111111111111111111 success"",
        ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
        ""Program ComputeBudget111111111111111111111111111111 success"",
        ""Program RVCEK6tHXkRgLFwK2oYyPyvT3nHcZ5CaFzTKCn96WCe invoke [1]"",
        ""Program log: Instruction: CreateUniversalOrder"",
        ""Program 11111111111111111111111111111111 invoke [2]"",
        ""Program 11111111111111111111111111111111 success"",
        ""Program 11111111111111111111111111111111 invoke [2]"",
        ""Program 11111111111111111111111111111111 success"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
        ""Program log: Instruction: InitializeAccount3"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4214 of 175110 compute units"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
        ""Program log: Buy Order Created: FiatGuy HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9 wants to buy 12000000 tokens"",
        ""Program data: moImVuZWj0GHse2VQgzqOJZAXYP7rKcsZnezixh1gK9uG3geOWCAPvfutU44Iq2EBXjSB34SdwSzhcDhUyFng3YV8mqoRuRg6Sg5VQll/9TWSsqvRtRd9zGOW09XyQxIfWBiXYKbg3sAABu3AAAAAAAMAAAAAAAAAPIUzXWZAQAAEWHjmVUMF0ypmkSKtAIFd4ijr5BjHfwavTGMGllftCmSYdJoAAAAAA=="",
        ""Program RVCEK6tHXkRgLFwK2oYyPyvT3nHcZ5CaFzTKCn96WCe consumed 44996 of 199700 compute units"",
        ""Program RVCEK6tHXkRgLFwK2oYyPyvT3nHcZ5CaFzTKCn96WCe success""
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
          ""programId"": ""RVCEK6tHXkRgLFwK2oYyPyvT3nHcZ5CaFzTKCn96WCe"",
          ""instruction"": {
            ""data"": ""XNNUgVA9bPpAWTG9rxioCBgoA5YJ2B7Dbk3u5vD5x2qWP"",
            ""accounts"": [
              {
                ""pubkey"": ""HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9"",
                ""preBalance"": 1922282160,
                ""postBalance"": 1917712960
              },
              {
                ""pubkey"": ""A8hQBWNWQWGUmPhFCFHaRcUNxWdueveBgbCcCYzdU5wT"",
                ""preBalance"": 0,
                ""postBalance"": 2449920
              },
              {
                ""pubkey"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""preBalance"": 85769688094,
                ""postBalance"": 85769688094
              },
              {
                ""pubkey"": ""2ArWFwaXmEsqhoQWpNK94WzHRUSLRkU2KrcsYPB9xqqr"",
                ""preBalance"": 0,
                ""postBalance"": 2039280
              },
              {
                ""pubkey"": ""5fnhKnUsstfSNa1ZqKscPvWPTPYALv1rNuXvt5Ts86jn"",
                ""preBalance"": 2039280,
                ""postBalance"": 2039280
              },
              {
                ""pubkey"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"",
                ""preBalance"": 6268187701,
                ""postBalance"": 6268187701
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