using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class RebalanceSessionRepository : ARepository<RebalanceSession>
{
    public RebalanceSessionRepository(ApplicationDbContext context) : base(context) { }
}
