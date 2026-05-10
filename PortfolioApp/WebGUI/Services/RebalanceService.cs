using Domain.interfaces;
using PortfolioApp.Enities;
using System.Text.Json;
using System.Net.Http.Json;

namespace WebGUI.Services;

public class RebalanceService
{
    private readonly IRepository<RebalanceSession> _sessionRepo;
    private readonly IRepository<RebalanceSwap> _swapRepo;
    private readonly IRepository<Wallet> _walletRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<TargetAllocation> _targetRepo;
    private readonly IRepository<CoinHolding> _holdingRepo;
    private readonly IHttpClientFactory _httpFactory;

    public RebalanceService(
        IRepository<RebalanceSession> sessionRepo,
        IRepository<RebalanceSwap> swapRepo,
        IRepository<Wallet> walletRepo,
        IRepository<User> userRepo,
        IRepository<TargetAllocation> targetRepo,
        IRepository<CoinHolding> holdingRepo,
        IHttpClientFactory httpFactory)
    {
        _sessionRepo = sessionRepo;
        _swapRepo = swapRepo;
        _walletRepo = walletRepo;
        _userRepo = userRepo;
        _targetRepo = targetRepo;
        _holdingRepo = holdingRepo;
        _httpFactory = httpFactory;
    }

   public List<RebalanceSwap> DetermineSwaps(string userEmail)
{
    var user = _userRepo.Read(u => u.Email == userEmail).First();
    var wallets = _walletRepo.Read(w => w.Email == userEmail).ToList();
    var walletAddresses = wallets.Select(w => w.Address).ToList();
    var holdings = _holdingRepo.Read(h =>
        walletAddresses.Contains(h.WalletAddress) && h.WalletEmail == userEmail
    ).ToList();
    var targets = _targetRepo.Read(t => t.UserEmail == userEmail).ToList();

    double totalUsd = holdings.Sum(h => (double)h.UsdValue);
    if (totalUsd == 0) return new();

    // Achtung: jetzt mit Tuple (UserEmail, TokenAddress)
    var targetDict = targets
        .GroupBy(t => (t.UserEmail, t.Address))
        .ToDictionary(
            g => g.Key,
            g => g.First().TargetPercent
        );

    var currentUsd = holdings
        .GroupBy(h => (userEmail, h.TokenAddress))
        .ToDictionary(
            g => g.Key,
            g => g.Sum(h => (double)h.UsdValue)
        );

    var holdingRest = holdings
        .GroupBy(h => (userEmail, h.TokenAddress, h.WalletAddress))
        .ToDictionary(
            g => (userEmail, g.Key.TokenAddress, g.Key.WalletAddress),
            g => new
            {
                Amount = g.Sum(h => (double)h.Amount),
                UsdValue = g.Sum(h => (double)h.UsdValue)
            });

    var swaps = new List<RebalanceSwap>();

    (Dictionary<string, double> deficit, Dictionary<string, double> excess) ComputeDiffs()
    {
        var deficit = new Dictionary<string, double>();
        var excess = new Dictionary<string, double>();
        foreach (var t in targets)
        {
            var key = (t.UserEmail, t.Address);
            double curr = currentUsd.GetValueOrDefault(key, 0.0);
            double want = totalUsd * t.TargetPercent / 100.0;
            if (curr < want - 5) deficit[t.Address] = want - curr;
            else if (curr > want + 5) excess[t.Address] = curr - want;
        }
        return (deficit, excess);
    }

    Func<List<KeyValuePair<string, double>>> getNonTargetHoldings = () =>
        currentUsd
            .Where(x => x.Key.Item1 == userEmail && !targetDict.ContainsKey(x.Key) && x.Value > 5)
            .Select(x => new KeyValuePair<string, double>(x.Key.Item2, x.Value))
            .OrderByDescending(x => x.Value)
            .ToList();

    while (true)
    {
        var (deficit, excess) = ComputeDiffs();
        if (deficit.Count == 0) break;

        bool anySwap = false;
        foreach (var def in deficit.OrderByDescending(x => x.Value))
        {
            var token = def.Key;
            var amountNeeded = def.Value;
            var targetSymbol = holdings.FirstOrDefault(h => h.TokenAddress == token)?.Symbol ?? "???";
            var targetAddress = holdings.FirstOrDefault(h => h.TokenAddress == token)?.TokenAddress ?? "???";
            var stillNeeded = amountNeeded;

            foreach (var over in excess.OrderByDescending(x => x.Value).ToList())
            {
                if (stillNeeded <= 5) break;
                if (over.Key == token || over.Value <= 5) continue;
                if (!currentUsd.ContainsKey((userEmail, over.Key))) continue;

                var srcHolding = holdingRest
                    .Where(x => x.Key.Item1 == userEmail && x.Key.Item2 == over.Key && x.Value.UsdValue > 0)
                    .OrderByDescending(x => x.Value.UsdValue)
                    .FirstOrDefault();
                if (srcHolding.Value == null) continue;

                var srcSymbol = holdings.FirstOrDefault(h => h.TokenAddress == over.Key)?.Symbol ?? "???";
                var srcAddress = holdings.FirstOrDefault(h => h.TokenAddress == over.Key)?.TokenAddress ?? "???";
                var swapUsd = Math.Min(over.Value, stillNeeded);
                swapUsd = Math.Min(swapUsd, srcHolding.Value.UsdValue);

                if (swapUsd < 5) continue;

                swaps.Add(new RebalanceSwap
                {
                    WalletId = srcHolding.Key.Item3,
                    FromSymbol = srcSymbol,
                    FromAddress = srcAddress,
                    ToSymbol = targetSymbol,
                    ToAddress = targetAddress,
                    Amount = (decimal)(swapUsd / (srcHolding.Value.UsdValue / srcHolding.Value.Amount)),
                    UsdValue = (decimal)swapUsd
                });

                currentUsd[(userEmail, over.Key)] -= swapUsd;
                if (!currentUsd.ContainsKey((userEmail, token))) currentUsd[(userEmail, token)] = 0.0;
                currentUsd[(userEmail, token)] += swapUsd;

                var holdingRef = holdingRest[(userEmail, over.Key, srcHolding.Key.Item3)];
                holdingRest[(userEmail, over.Key, srcHolding.Key.Item3)] = new
                {
                    Amount = holdingRef.Amount - (swapUsd / (srcHolding.Value.UsdValue / srcHolding.Value.Amount)),
                    UsdValue = holdingRef.UsdValue - swapUsd
                };
                stillNeeded -= swapUsd;
                anySwap = true;
            }

            foreach (var src in getNonTargetHoldings())
            {
                if (stillNeeded <= 5) break;
                if (src.Value <= 5) continue;

                var srcHolding = holdingRest
                    .Where(x => x.Key.Item1 == userEmail && x.Key.Item2 == src.Key && x.Value.UsdValue > 0)
                    .OrderByDescending(x => x.Value.UsdValue)
                    .FirstOrDefault();
                if (srcHolding.Value == null) continue;

                var srcSymbol = holdings.FirstOrDefault(h => h.TokenAddress == src.Key)?.Symbol ?? "???";
                var srcAddress = holdings.FirstOrDefault(h => h.TokenAddress == src.Key)?.TokenAddress ?? "???";
                var swapUsd = Math.Min(src.Value, stillNeeded);
                swapUsd = Math.Min(swapUsd, srcHolding.Value.UsdValue);

                if (swapUsd < 5) continue;

                swaps.Add(new RebalanceSwap
                {
                    WalletId = srcHolding.Key.Item3,
                    FromSymbol = srcSymbol,
                    FromAddress = srcAddress,
                    ToSymbol = targetSymbol,
                    ToAddress = targetAddress,
                    Amount = (decimal)(swapUsd / (srcHolding.Value.UsdValue / srcHolding.Value.Amount)),
                    UsdValue = (decimal)swapUsd
                });

                currentUsd[(userEmail, src.Key)] -= swapUsd;
                if (!currentUsd.ContainsKey((userEmail, token))) currentUsd[(userEmail, token)] = 0.0;
                currentUsd[(userEmail, token)] += swapUsd;

                var holdingRef = holdingRest[(userEmail, src.Key, srcHolding.Key.Item3)];
                holdingRest[(userEmail, src.Key, srcHolding.Key.Item3)] = new
                {
                    Amount = holdingRef.Amount - (swapUsd / (srcHolding.Value.UsdValue / srcHolding.Value.Amount)),
                    UsdValue = holdingRef.UsdValue - swapUsd
                };
                stillNeeded -= swapUsd;
                anySwap = true;
            }
        }

        if (!anySwap) break;
    }

    return swaps;
}

    public void ConfirmSession(int sessionId)
    {
        var session = _sessionRepo.Read(sessionId);
        if (session != null && !session.IsConfirmed)
        {
            session.IsConfirmed = true;
            _sessionRepo.Update(session);
        }
    }

    public async Task<string> BuildSwapTransactionAsync(RebalanceSwap swap, string userPublicKey, string network)
    {
        var client = _httpFactory.CreateClient();

        var fromHolding = _holdingRepo.Read(h => h.TokenAddress == swap.FromAddress && h.WalletAddress == swap.WalletId).FirstOrDefault();
        var toHolding = _holdingRepo.Read(h => h.TokenAddress == swap.ToAddress && h.WalletAddress == swap.WalletId).FirstOrDefault();

        if (fromHolding == null || fromHolding.Decimals < 0)
            throw new Exception($"From CoinHolding fehlerhaft: {fromHolding?.Symbol}");

        if (toHolding == null)
            throw new Exception($"To CoinHolding nicht gefunden für {swap.ToSymbol}");

        ulong amountLamports = (ulong)Math.Floor((double)swap.Amount * Math.Pow(10, fromHolding.Decimals));
        if (amountLamports == 0) amountLamports = 1;

        var quoteUrl = $"https://quote-api.jup.ag/v6/quote?inputMint={fromHolding.TokenAddress}&outputMint={toHolding.TokenAddress}&amount={amountLamports}&slippageBps=50";
        var quoteResponse = await client.GetAsync(quoteUrl);
        if (!quoteResponse.IsSuccessStatusCode)
            throw new Exception($"Quote API fehlgeschlagen: {await quoteResponse.Content.ReadAsStringAsync()}");

        var quoteRoot = JsonDocument.Parse(await quoteResponse.Content.ReadAsStringAsync()).RootElement;

        var payload = new
        {
            quoteResponse = quoteRoot,
            userPublicKey,
            wrapAndUnwrapSol = true,
            asLegacyTransaction = false
        };

        var resp = await client.PostAsJsonAsync("https://quote-api.jup.ag/v6/swap", payload);
        resp.EnsureSuccessStatusCode();

        var swapJson = await resp.Content.ReadAsStringAsync();
        return JsonDocument.Parse(swapJson).RootElement.GetProperty("swapTransaction").GetString();
    }

    public void RecordExecution(int swapId, string txHash)
    {
        var swap = _swapRepo.Read(swapId);
        if (swap != null)
        {
            swap.TxHash = txHash;
            _swapRepo.Update(swap);
        }
    }
}