using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;
[Table("rebalancesessions")]
public class RebalanceSession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("useremail", TypeName = "varchar(255)")]
    public string UserEmail { get; set; } = null!;

    [Column("createdat", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("isconfirmed", TypeName = "bit")]
    public bool IsConfirmed { get; set; }

    [Required]
    [Column("token", TypeName = "varchar(255)")]
    public string Token { get; set; } = null!;
    
    public User User { get; set; }
    public ICollection<RebalanceSwap> RebalanceSwaps { get; set; }
}