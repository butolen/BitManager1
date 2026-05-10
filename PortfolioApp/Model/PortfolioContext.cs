using PortfolioApp.Enities;


using Microsoft.EntityFrameworkCore;


namespace Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets
     
        public DbSet<Chain> Chains { get; set; }
        public DbSet<PendingUser> PendingUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<CoinHolding> CoinHoldings { get; set; }
        public DbSet<TargetAllocation> TargetAllocations { get; set; }
        public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }
        public DbSet<RebalanceSession> RebalanceSessions { get; set; }
        public DbSet<RebalanceSwap> RebalanceSwaps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table: Wallet -> User
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(w => w.Email);

            modelBuilder.Entity<CoinHolding>()
                .HasKey(c => new { c.WalletAddress, c.WalletEmail, c.TokenAddress });

            modelBuilder.Entity<CoinHolding>()
                .HasOne(c => c.Wallet)
                .WithMany(w => w.CoinHoldings)
                .HasForeignKey(c => new { c.WalletAddress, c.WalletEmail });

            // Table: TargetAllocation -> User
            modelBuilder.Entity<TargetAllocation>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.TargetAllocations)
                .HasForeignKey(ta => ta.UserEmail);

            // Table: PortfolioSnapshot -> User
            modelBuilder.Entity<PortfolioSnapshot>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.PortfolioSnapshots)
                .HasForeignKey(ps => ps.UserEmail);

            // Table: RebalanceSession -> User
            modelBuilder.Entity<RebalanceSession>()
                .HasOne(rs => rs.User)
                .WithMany(u => u.RebalanceSessions)
                .HasForeignKey(rs => rs.UserEmail);

            // Table: RebalanceSwap -> RebalanceSession
            modelBuilder.Entity<RebalanceSwap>()
                .HasOne(swap => swap.Session)
                .WithMany(session => session.RebalanceSwaps)
                .HasForeignKey(swap => swap.SessionId);

            modelBuilder.Entity<RebalanceSwap>()
                .HasOne(swap => swap.Wallet)
                .WithMany(wallet => wallet.RebalanceSwaps)
                .HasForeignKey(swap => new { swap.WalletId, swap.WalletEmail })
                .HasPrincipalKey(wallet => new { wallet.Address, wallet.Email });

            
            // Composite Key für Wallet: Address + Email
            modelBuilder.Entity<Wallet>()
                .HasKey(w => new { w.Address, w.Email });
        }
    }
}