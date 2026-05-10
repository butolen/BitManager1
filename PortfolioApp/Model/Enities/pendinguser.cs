using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioApp.Enities;

[Table("pending_users")]
public class PendingUser
{
    [Key]
    public string Token { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string PlainPassword { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}