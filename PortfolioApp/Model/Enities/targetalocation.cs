using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("targetallocations")]
public class TargetAllocation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("useremail", TypeName = "varchar(255)")]
    public string UserEmail { get; set; } = null!;

    [Required]
    [Column("symbol", TypeName = "varchar(200)")]
    public string Symbol { get; set; } = null!;

    [Required]
    [Column("address", TypeName = "varchar(200)")]
    public string Address{ get; set; } = null!;
    
    [Column("currentallocationpercent", TypeName = "double")]
    public double CurrentAllocationPercent { get; set; } // automatisch berechnet


    [Column("targetpercent", TypeName = "double")]
    public double TargetPercent { get; set; } 

    
    [Column("tolerancepercent", TypeName = "double")]
    public double TolerancePercent { get; set; }
    
    public User User { get; set; }
}