using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class ChainRepository : ARepository<Chain>
{
    public ChainRepository(ApplicationDbContext context) : base(context) { }

   
}