using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("wallets")]
public class Wallet
{
    
    [Column("address", TypeName = "varchar(100)")]
    public string Address { get; set; } = null!;

    [Required]
    [Column("email", TypeName = "varchar(255)")]
    public string Email { get; set; } = null!;

    [Column("usdvalue", TypeName = "decimal(18,8)")]
    public decimal UsdValue { get; set; }  
    [Required]
    [Column("network", TypeName = "varchar(100)")]
    public string Network { get; set; } = null!;
    [Column("lastupdated")]
    public DateTime LastUpdated { get; set; }
    
    public User User { get; set; }
    public ICollection<CoinHolding> CoinHoldings { get; set; }
    public ICollection<RebalanceSwap> RebalanceSwaps { get; set; }
}