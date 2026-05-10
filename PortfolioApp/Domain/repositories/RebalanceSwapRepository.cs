using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class RebalanceSwapRepository : ARepository<RebalanceSwap>
{
    public RebalanceSwapRepository(ApplicationDbContext context) : base(context) { }
}
