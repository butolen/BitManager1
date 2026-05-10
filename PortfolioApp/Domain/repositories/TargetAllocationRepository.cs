using Domain.interfaces;
using Model;
using PortfolioApp.Enities;

namespace Domain.repositories;

public class TargetAllocationRepository : ARepository<TargetAllocation>
{
    public TargetAllocationRepository(ApplicationDbContext context) : base(context)
    {
    }
}