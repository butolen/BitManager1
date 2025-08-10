using Domain.interfaces;
using PortfolioApp.Enities;

public class DashboardService
{
    private readonly IRepository<CoinHolding> _coinHoldingRepository;
    private readonly IRepository<TargetAllocation> _targetAllocationRepository;

    public DashboardService(
        IRepository<CoinHolding> coinHoldingRepository,
        IRepository<TargetAllocation> targetAllocationRepository)
    {
        _coinHoldingRepository = coinHoldingRepository;
        _targetAllocationRepository = targetAllocationRepository;
    }

    public List<CoinSummary> GetAggregatedCoinHoldingsForUser(string userEmail)
    {
        var holdings = _coinHoldingRepository
            .Read(ch => ch.Wallet.Email == userEmail);

        var allocations = _targetAllocationRepository
            .Read(ta => ta.User.Email == userEmail)
            .ToDictionary(ta => ta.Address, ta => ta.CurrentAllocationPercent);

        var grouped = holdings
            .GroupBy(ch => ch.Symbol)
            .Select(g =>
            {
                var tokenAddress = g.First().TokenAddress;
                allocations.TryGetValue(tokenAddress, out var allocation);

                return new CoinSummary
                {
                    Symbol = g.Key,
                    TotalUsdValue = g.Sum(ch => ch.UsdValue),
                    TokenName = g.First().TokenName,
                    ImageUrl = g.First().ImageUrl,
                    CurrentAllocationPercent = allocation
                };
            })
            .OrderByDescending(cs => cs.TotalUsdValue)
            .ToList();

        return grouped;
    }
    public List<CoinHolding> GetUniqueHoldingsByAddressForUser(string userEmail)
    {
        var all = _coinHoldingRepository
            .Read(ch => ch.Wallet != null && ch.Wallet.Email == userEmail);

        var grouped = all
            .GroupBy(ch => ch.TokenAddress)
            .Select(g => g.First()) // beliebiger Eintrag pro TokenAddress
            .ToList();

        return grouped;
    }

    public decimal GetTotalPortfolioValue(string userEmail)
    {
        return _coinHoldingRepository
            .Read(ch => ch.Wallet.Email == userEmail)
            .Sum(ch => ch.UsdValue);
    }
}
public class CoinSummary
{
    public string Symbol { get; set; }
    public decimal TotalUsdValue { get; set; }
    public string TokenName { get; set; }
    public string ImageUrl { get; set; }
    public double CurrentAllocationPercent { get; set; }
}  