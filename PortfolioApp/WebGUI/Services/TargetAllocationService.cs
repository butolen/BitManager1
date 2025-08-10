using System.Net;
using System.Net.Mail;
using Domain.interfaces;
using PortfolioApp.Enities;

namespace WebGUI.Services;

public class TargetAllocationService
{
    private readonly IRepository<TargetAllocation> _repo;

    public TargetAllocationService(IRepository<TargetAllocation> repo)
    {
        _repo = repo;
    }

    public List<TargetAllocation> GetViolatedAllocations(string userEmail, List<CoinHolding> holdings)
    {
        var allocations = _repo.Read(t => t.UserEmail == userEmail).ToList();

        var result = new List<TargetAllocation>();

        var totalValue = holdings.Sum(h => h.UsdValue);

        foreach (var alloc in allocations.Where(a => a.TargetPercent > 0))
        {
            var matching = holdings.FirstOrDefault(h => h.TokenAddress == alloc.Address);
            if (matching == null) continue;
            var actualPercent = totalValue > 0 ? (double)(matching.UsdValue / totalValue * 100) : 0;

            var diff = Math.Abs(actualPercent - alloc.TargetPercent);
            if (diff > alloc.TolerancePercent)
                result.Add(alloc);
        }

        return result;
    }

    public void SetAllocation(string userEmail, string symbol, string address, double target, double tolerance)
    {
        var existing = _repo.Read(t => t.UserEmail == userEmail && t.Symbol == symbol).FirstOrDefault();
        if (existing != null)
        {
            existing.TargetPercent = target;
            existing.TolerancePercent = tolerance;
            _repo.Update(existing);
        }
        else
        {
            var allocation = new TargetAllocation
            {
                UserEmail = userEmail,
                Symbol = symbol,
                Address = address,
                TargetPercent = target,
                TolerancePercent = tolerance
            };
            _repo.Create(allocation);
        }
    }
    public void DeleteAllocation(int id)
    {
        var existing = _repo.Read(t => t.Id == id).FirstOrDefault();
        if (existing != null)
        {
            _repo.Delete(existing);
        }
    }
    public void SetGlobalTolerance(string userEmail, double globalTolerance)
    {
        var targets = _repo.Read(t => t.UserEmail == userEmail);
        foreach (var target in targets)
        {
            target.TolerancePercent = globalTolerance;
            _repo.Update(target);
        }
    }
    public List<TargetAllocation> GetAllocations(string userEmail)
        => _repo.Read(t => t.UserEmail == userEmail && t.TargetPercent > 0);
    public TargetAllocation GetOrCreate(string userEmail, string symbol, string address)
    {
        var existing = _repo
            .Read(t => t.UserEmail == userEmail && t.Symbol == symbol && t.Address == address)
            .FirstOrDefault();

        if (existing != null)
            return existing;

        var newAllocation = new TargetAllocation
        {
            UserEmail = userEmail,
            Symbol = symbol,
            Address = address,
            TargetPercent = 0,
            TolerancePercent = 5,
            CurrentAllocationPercent = 0
        };

        _repo.Create(newAllocation);
        return newAllocation;
    }
    public void Update(TargetAllocation ta)
    {
        _repo.Update(ta);
    }
}