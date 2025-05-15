namespace Tests_back.Extensions;

public static class TestJson
{
    public const string OfferInitialized = @"
    {
  ""data"": [
    {
      ""blockTime"": 1747314439,
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
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA consumed 4645 of 162987 compute units"",
        ""Program TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA success"",
        ""Program log: Offer created by FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ, amount: 1000000"",
        ""Program data: TWIEKZ3vRy7cFIKq46W0Z30FafmUKBty1XIOH6naXzPSAVxKGuawOtWoq1IRzHxG2UyjPwTU7hQ8559NFYympQkpo5dMdLzQ6Sg5VQll/9TWSsqvRtRd9zGOW09XyQxIfWBiXYKbg3tVU0QAAAAAAEBCDwAAAAAALAEAAAAAAABtVw7UlgEAAAfnJWgAAAAAAA=="",
        ""Program 9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN consumed 32996 of 177740 compute units"",
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
                ""pubkey"": ""3xhnhxZeYdWW15GoHNvh2udEri4SnfX67SvryWH8vsfk""
              },
              {
                ""postBalance"": 2039280,
                ""preBalance"": 0,
                ""pubkey"": ""7dEhSghJj1y4TthhCEn8rfsZFSikXrhSczisgKBisLHG""
              },
              {
                ""postBalance"": 71737584094,
                ""preBalance"": 71737584094,
                ""pubkey"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr""
              },
              {
                ""postBalance"": 3880189547,
                ""preBalance"": 3884334187,
                ""pubkey"": ""FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ""
              },
              {
                ""postBalance"": 1,
                ""preBalance"": 1,
                ""pubkey"": ""11111111111111111111111111111111""
              }
            ],
            ""data"": {
              ""info"": {
                ""account"": ""7dEhSghJj1y4TthhCEn8rfsZFSikXrhSczisgKBisLHG"",
                ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""source"": ""FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ"",
                ""systemProgram"": ""11111111111111111111111111111111"",
                ""tokenProgram"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA"",
                ""wallet"": ""3xhnhxZeYdWW15GoHNvh2udEri4SnfX67SvryWH8vsfk""
              },
              ""type"": ""create""
            },
            ""index"": 2,
            ""tokenBalances"": [
              {
                ""accountIndex"": 1,
                ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""owner"": ""3xhnhxZeYdWW15GoHNvh2udEri4SnfX67SvryWH8vsfk"",
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
                ""postBalance"": 2025360,
                ""preBalance"": 0,
                ""pubkey"": ""Fp6qKJZBoYRnSgDC3yt6DDUfzZXzEpnq2r6WxKFiczws""
              },
              {
                ""postBalance"": 2039280,
                ""preBalance"": 2039280,
                ""pubkey"": ""eD6g3xKvU9s8f3VNkzAo2QRbByhVpSFovT1wwnQ1rRx""
              },
              {
                ""postBalance"": 2039280,
                ""preBalance"": 0,
                ""pubkey"": ""7dEhSghJj1y4TthhCEn8rfsZFSikXrhSczisgKBisLHG""
              },
              {
                ""postBalance"": 3880189547,
                ""preBalance"": 3884334187,
                ""pubkey"": ""FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ""
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
            ""data"": ""cabmptwkPkhQ8fHG45fpoTKVEe3r1zhFvimSLjiH52iGyvATjRb3xLaEGJqoVs5qf6ErWxk39HjttzDYuSWV1mWppRpv64UKF5"",
            ""index"": 3,
            ""tokenBalances"": [
              {
                ""accountIndex"": 1,
                ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""owner"": ""3xhnhxZeYdWW15GoHNvh2udEri4SnfX67SvryWH8vsfk"",
                ""uiTokenAmount"": {
                  ""amount"": ""1000000"",
                  ""decimals"": 6,
                  ""uiAmount"": 1
                }
              },
              {
                ""accountIndex"": 2,
                ""mint"": ""Gh9ZwEmdLJ8DscKNTkTqPbNwLNNBjuSzaG9Vp2KGtKJr"",
                ""owner"": ""FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ"",
                ""uiTokenAmount"": {
                  ""amount"": ""987000000"",
                  ""decimals"": 6,
                  ""uiAmount"": 987
                }
              }
            ]
          },
          ""programId"": ""9LEGQe2obd9UizPkci9zHwrQbF7js5J4p92wn5b83ALN""
        }
      ],
      ""signature"": ""5SB8uVLRPJ4fgzCNZnJGSQjSjSDXExGXCn6egtaSEpxtQYyzmqea2arkskPDu36A8xQfKddyUTve8Rm8SkJTtkGT"",
      ""slot"": 381033945,
      ""success"": true
    }
  ]
}   
";
}