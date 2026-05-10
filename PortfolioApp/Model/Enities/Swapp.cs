using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("rebalanceswaps")]
public class RebalanceSwap
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("sessionid")]
    public int SessionId { get; set; }

    [Required]
    [Column("walletid", TypeName = "varchar(100)")]
    public string WalletId { get; set; } = null!;

    [Required]
    [Column("walletemail", TypeName = "varchar(255)")]
    public string WalletEmail { get; set; } = null!;

    [Required]
    [Column("fromsymbol", TypeName = "varchar(20)")]
    public string FromSymbol { get; set; } = null!;

    [Required]
    [Column("tosymbol", TypeName = "varchar(20)")]
    public string ToSymbol { get; set; } = null!;

    [Required]
    [Column("fromaddress", TypeName = "varchar(20)")]
    public string FromAddress { get; set; } = null!;

    [Required]
    [Column("toaddress", TypeName = "varchar(20)")]
    public string ToAddress { get; set; } = null!;

    [Required]
    [Column("amount", TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [Column("usdvalue", TypeName = "decimal(18,8)")]
    public decimal UsdValue { get; set; }

    [Column("txhash", TypeName = "varchar(100)")]
    public string? TxHash { get; set; }

    public RebalanceSession Session { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
}