using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("coinholdings")]
public class CoinHolding
{
    [Key, Column("walletaddress", Order = 0, TypeName = "varchar(100)")]
    public string WalletAddress { get; set; } = null!;

    [Key, Column("walletemail", Order = 1, TypeName = "varchar(255)")]
    public string WalletEmail { get; set; } = null!;

    [Key, Column("tokenaddress", Order = 2, TypeName = "varchar(100)")]
    public string TokenAddress { get; set; } = null!;

    [Column("symbol", TypeName = "varchar(20)")]
    public string Symbol { get; set; } = null!;

    [Column("decimals", TypeName = "int")]
    public int Decimals { get; set; }

    [Column("amount", TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Column("usdvalue", TypeName = "decimal(18,8)")]
    public decimal UsdValue { get; set; }

    [Column("TokenName", TypeName = "varchar(255)")]
    public string TokenName { get; set; } = null!;

    [Column("ImageUrl", TypeName = "varchar(1000)")]
    public string ImageUrl { get; set; } = null!;

    // Navigation Property
    public Wallet Wallet { get; set; } = null!;
}