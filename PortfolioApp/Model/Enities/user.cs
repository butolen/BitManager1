using Model.Entities;

namespace PortfolioApp.Enities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public class User
{
    [Key]
    [EmailAddress]
    [Column("email", TypeName = "varchar(255)")]
    public string Email { get; set; } = null!;

    [Required]
    [Column("hashedpassword", TypeName = "varchar(255)")]
    public string HashedPassword { get; set; } = null!;

    [Required]
    [Column("role", TypeName = "int")]
    public Role Role { get; set; }

    [Column("twofactorenabled", TypeName = "bit")]
    public bool TwoFactorEnabled { get; set; }
    [Column("globaltolerancepercent", TypeName = "decimal(5,2)")]
    public double? GlobalTolerancePercent { get; set; } = 5.0;

    [Column("notifyondeviation", TypeName = "bit")]
    public bool NotifyOnDeviation { get; set; } = true;

    [Column("autodeterminestrategy", TypeName = "bit")]
    public bool AutoDetermineStrategy { get; set; } = true;

    [Column("emailcooldownenabled", TypeName = "bit")]
    public bool EmailCooldownEnabled { get; set; } = false;

    [Column("emailcooldownhours", TypeName = "int")]
    public int EmailCooldownHours { get; set; } = 24;
    [Column("lastdriftemail", TypeName = "datetime(6)")]
    public DateTime? LastDriftEmail { get; set; }
    public string? TwoFactorCode { get; set; }
    public DateTime? TwoFactorExpiresAt { get; set; }
    public double MinimumSwappInUSD { get; set; } = 5;
    public string? UserProfileImage { get; set; }
    [Column("twofactortemptoken", TypeName = "varchar(255)")]
    public string? TwoFactorTempToken { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? TokenGeneratedAt { get; set; }
    
    public string? PasswordResetToken { get; set; } 
    public ICollection<Wallet> Wallets { get; set; }
    public ICollection<TargetAllocation> TargetAllocations { get; set; }
    public ICollection<PortfolioSnapshot> PortfolioSnapshots { get; set; }
    public ICollection<RebalanceSession> RebalanceSessions { get; set; }
}