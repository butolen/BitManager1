using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("portfoliosnapshots")]
public class PortfolioSnapshot
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("useremail", TypeName = "varchar(255)")]
    public string UserEmail { get; set; } = null!;

    [Column("timestamp", TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [Column("totalvalueusd", TypeName = "decimal(18,8)")]
    public decimal TotalValueUsd { get; set; }
    public User User { get; set; }
}