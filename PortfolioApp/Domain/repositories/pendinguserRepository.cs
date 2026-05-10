using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class PendingUserRepository : ARepository<PendingUser>
{
    public PendingUserRepository(ApplicationDbContext context) : base(context)
    {
    }
}