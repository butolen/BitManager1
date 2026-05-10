using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//aktuell keine verwendung ist aber wichtig für multi cain support 
namespace PortfolioApp.Enities;
[Table("chains")]
public class Chain
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = null!; // z.B. "solana", "ethereum"

    [Column("rpcurl")]
    public string? RpcUrl { get; set; }

    [Column("swapapi")]
    public string? SwapApi { get; set; }

   
}