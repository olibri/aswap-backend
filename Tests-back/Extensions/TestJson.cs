namespace Tests_back.Extensions;

public static class TestJson
{
    public const string OfferInitialized = @"
    {
      ""data"": [
        {
          ""blockTime"": 1745946188,
          ""logs"": [
            ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
            ""Program ComputeBudget111111111111111111111111111111 success"",
            ""Program ComputeBudget111111111111111111111111111111 invoke [1]"",
            ""Program ComputeBudget111111111111111111111111111111 success"",
            ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL invoke [1]"",
            ""Program log: Create"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
            ""Program log: Instruction: GetAccountDataSize"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 1595 of 192755 compute units"",
            ""Program return: TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA pQAAAAAAAAA="",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
            ""Program 11111111111111111111111111111111 invoke [2]"",
            ""Program 11111111111111111111111111111111 success"",
            ""Program log: Initialize the associated token account"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
            ""Program log: Instruction: InitializeImmutableOwner"",
            ""Program log: Please upgrade to SPL Token 2022 for immutable owner support"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 1405 of 186142 compute units"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
            ""Program log: Instruction: InitializeAccount3"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4214 of 182258 compute units"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
            ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL consumed 21960 of 199700 compute units"",
            ""Program ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL success"",
            ""Program 9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN invoke [1]"",
            ""Program log: Instruction: InitializeOffer"",
            ""Program 11111111111111111111111111111111 invoke [2]"",
            ""Program 11111111111111111111111111111111 success"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA invoke [2]"",
            ""Program log: Instruction: Transfer"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4645 of 163047 compute units"",
            ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
            ""Program log: Offer created by e8gsd2rHb5uJTKYoUJM9i9Q113HtmNsJ9gcMJnAUdyq, amount: 1000000"",
            ""Program data: TWIEKZ3vRy6k/QcZt9WiIz+EPzv9JuFbIy6InNBTdgb8qM+A2I4C8AmDLqMdziw99f6YS+52/qNbNPu1/go44uQh9+KSocgY6Sg5VQll/9TWSsqvRtRd9zGOW09XyQxIfWBiXYKbg3tVQUgAAAAAAEBCDwAAAAAAJKkAAAAAAABPcoCClgEAAEwGEWgAAAAA"",
            ""Program 9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN consumed 32534 of 177740 compute units"",
            ""Program 9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN success""
          ],
          ""programInvocations"": [
            {
              ""instruction"": {
                ""accounts"": [
                  {
                    ""postBalance"": 934087680,
                    ""preBalance"": 934087680,
                    ""pubkey"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA""
                  },
                  {
                    ""postBalance"": 0,
                    ""preBalance"": 0,
                    ""pubkey"": ""J6abqiniFzaT76h4JMuaPeQmncPMnQ3Ac3inwM5uC2Mj""
                  },
                  {
                    ""postBalance"": 2039280,
                    ""preBalance"": 0,
                    ""pubkey"": ""7gK3Ej7NdHUJr5FtHtpwwSrgb79bo6XrxzEKVLykwiPf""
                  },
                  {
                    ""postBalance"": 71737584094,
                    ""preBalance"": 71737584094,
                    ""pubkey"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr""
                  },
                  {
                    ""postBalance"": 5186771857,
                    ""preBalance"": 5220835360,
                    ""pubkey"": ""e8gsd2rHb5uJTKYoUJM9i9Q113HtmNsJ9gcMJnAUdyq""
                  },
                  {
                    ""postBalance"": 1,
                    ""preBalance"": 1,
                    ""pubkey"": ""11111111111111111111111111111111""
                  }
                ],
                ""data"": {
                  ""info"": {
                    ""account"": ""7gK3Ej7NdHUJr5FtHtpwwSrgb79bo6XrxzEKVLykwiPf"",
                    ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                    ""source"": ""e8gsd2rHb5uJTKYoUJM9i9Q113HtmNsJ9gcMJnAUdyq"",
                    ""systemProgram"": ""11111111111111111111111111111111"",
                    ""tokenProgram"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"",
                    ""wallet"": ""J6abqiniFzaT76h4JMuaPeQmncPMnQ3Ac3inwM5uC2Mj""
                  },
                  ""type"": ""create""
                },
                ""index"": 2,
                ""tokenBalances"": [
                  {
                    ""accountIndex"": 2,
                    ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                    ""owner"": ""J6abqiniFzaT76h4JMuaPeQmncPMnQ3Ac3inwM5uC2Mj"",
                    ""uiTokenAmount"": {
                      ""amount"": ""1000000"",
                      ""decimals"": 6,
                      ""uiAmount"": 1
                    }
                  }
                ]
              },
              ""programId"": ""ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL""
            },
            {
              ""instruction"": {
                ""accounts"": [
                  {
                    ""postBalance"": 2018400,
                    ""preBalance"": 0,
                    ""pubkey"": ""C73eCphTS1ZuEE5Lxg379fnVbzcbfSEbNrQaZqow9uFM""
                  },
                  {
                    ""postBalance"": 2039280,
                    ""preBalance"": 2039280,
                    ""pubkey"": ""7gAe4okaaUxfywaibgNVTvkPMNq4AZGiN5y7LJGWmis1""
                  },
                  {
                    ""postBalance"": 2039280,
                    ""preBalance"": 0,
                    ""pubkey"": ""7gK3Ej7NdHUJr5FtHtpwwSrgb79bo6XrxzEKVLykwiPf""
                  },
                  {
                    ""postBalance"": 5186771857,
                    ""preBalance"": 5220835360,
                    ""pubkey"": ""e8gsd2rHb5uJTKYoUJM9i9Q113HtmNsJ9gcMJnAUdyq""
                  },
                  {
                    ""postBalance"": 934087680,
                    ""preBalance"": 934087680,
                    ""pubkey"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA""
                  },
                  {
                    ""postBalance"": 1,
                    ""preBalance"": 1,
                    ""pubkey"": ""11111111111111111111111111111111""
                  }
                ],
                ""data"": ""94XzHHkfqCiKqQdJuopmMwuDjxyNysECYCjEu3t3HKWk3UTAEEj9pt7AoWsUkhM8Q8XnCmoTJSn7jyR7CCFYVhqtEtP6GSWgX"",
                ""index"": 3,
                ""tokenBalances"": [
                  {
                    ""accountIndex"": 1,
                    ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                    ""owner"": ""e8gsd2rHb5uJTKYoUJM9i9Q113HtmNsJ9gcMJnAUdyq"",
                    ""uiTokenAmount"": {
                      ""amount"": ""910000000"",
                      ""decimals"": 6,
                      ""uiAmount"": 910
                    }
                  },
                  {
                    ""accountIndex"": 2,
                    ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                    ""owner"": ""J6abqiniFzaT76h4JMuaPeQmncPMnQ3Ac3inwM5uC2Mj"",
                    ""uiTokenAmount"": {
                      ""amount"": ""1000000"",
                      ""decimals"": 6,
                      ""uiAmount"": 1
                    }
                  }
                ]
              },
              ""programId"": ""9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN""
            }
          ],
          ""signature"": ""5gMPjNVxAFhYmFaVvfPMC11hfAJcP5yy7f1TpZMTypy5gZYhyxXx7KQmHsh4paPp6J2JYk6TmgUz4784jWBYyna8"",
          ""slot"": 377560612,
          ""success"": true
        }
      ]
    }";
}