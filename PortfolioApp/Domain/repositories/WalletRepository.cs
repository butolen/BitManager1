using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class WalletRepository : ARepository<Wallet>
{
    public WalletRepository(ApplicationDbContext context) : base(context) { }

   
}