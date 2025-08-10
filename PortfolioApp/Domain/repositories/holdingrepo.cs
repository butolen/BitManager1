using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class CoinHoldingRepository : ARepository<CoinHolding>
{
    public CoinHoldingRepository(ApplicationDbContext context) : base(context) { }

   
}