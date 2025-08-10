using System.Globalization;
using Domain.interfaces;
using PortfolioApp.Enities;
using System.Net.Http;
using System.Net.Mail;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebGUI.Settings;

namespace WebGUI.Services
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IRepository<CoinHolding> _holdingRepo;
        private readonly IRepository<User> _userRepo;
        private readonly TargetAllocationService _allocationService;
        private readonly IHttpClientFactory _http;
        private readonly SolanaTrackerSettings _tracker;

        public WalletService(
            IRepository<Wallet> walletRepo,
            IRepository<CoinHolding> holdingRepo,
            IRepository<User> userRepo,
            TargetAllocationService allocationService,
            IHttpClientFactory http,
            IOptions<SolanaTrackerSettings> trackerOptions
        )
        {
            _walletRepo = walletRepo;
            _holdingRepo = holdingRepo;
            _userRepo = userRepo;
            _allocationService = allocationService;
            _http = http;
            _tracker = trackerOptions.Value;
        }

        private void RecalculateAllocationPercentages(string userEmail)
        {
            var userHoldings = _holdingRepo.Read(h => h.Wallet.Email == userEmail);
            if (userHoldings.Count == 0)
                return;

            var total = userHoldings.Sum(h => h.UsdValue);
            var grouped = userHoldings.GroupBy(h => h.TokenAddress);

            foreach (var group in grouped)
            {
                var tokenValue = group.Sum(h => h.UsdValue);
                double percent = total > 0 ? (double)(tokenValue / total * 100) : 0;

                var sample = group.First();
                var target = _allocationService.GetOrCreate(
                    userEmail,
                    sample.Symbol,
                    sample.TokenAddress
                );

                target.CurrentAllocationPercent = percent;
                _allocationService.Update(target);
            }
        }

        private void SendDriftEmail(string to, List<TargetAllocation> driftedTokens)
        {
            string rebalanceUrl = $"http://localhost:5281/portfolio/rebalance?user={Uri.EscapeDataString(to)}";

            string holdingsTable = "<ul style='padding-left:20px;'>";
            foreach (var token in driftedTokens)
            {
                holdingsTable += $"<li><strong>{token.Symbol}</strong>: " +
                                 $"Current: {token.CurrentAllocationPercent.ToString("F2")}%";
                if (token.TargetPercent!=0 )
                {
                    holdingsTable += $", Target: {token.TargetPercent:F2}% ± {token.TolerancePercent.ToString("F2")}%";
                }
                holdingsTable += "</li>";
            }
            holdingsTable += "</ul>";

            string htmlBody = $@"
<!DOCTYPE html>
<html lang='en'>
<head><meta charset='UTF-8' /></head>
<body style='background:#f8f9fa;font-family:Arial,sans-serif;'>
<div style='max-width:600px;margin:40px auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 0 10px rgba(0,0,0,0.1);'>
    <h2 style='color:#212529;'>📉 Portfolio Drift Detected</h2>
    <p>Your portfolio has drifted from the target allocation. Here are the affected tokens:</p>
    {holdingsTable}
    <div style='text-align:center;margin-top:30px;'>
        <a href='{rebalanceUrl}' style='background:#0d6efd;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold;'>Rebalance Now</a>
    </div>
    <p style='font-size:13px;color:#6c757d;margin-top:40px;'>If this wasn't expected, you can ignore this email.</p>
</div>
</body>
</html>";

            var mail = new MailMessage("mathiasbutolen@gmail.com", to, "Portfolio Drift Detected", htmlBody)
            {
                IsBodyHtml = true
            };

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("mathiasbutolen@gmail.com", "tlzbwhyawsugzqlc"),
                EnableSsl = true
            };

            smtp.Send(mail);
        }

        public void CheckAndNotifyDeviation(string userEmail)
        {
            var user = _userRepo.Read(u => u.Email == userEmail).FirstOrDefault();
            if (user == null || !user.NotifyOnDeviation)
                return;

            var holdings = _holdingRepo.Read(h => h.Wallet.Email == userEmail).ToList();
            var violations = _allocationService.GetViolatedAllocations(userEmail, holdings);
            if (!violations.Any())
                return;

            if (user.EmailCooldownEnabled && user.LastDriftEmail.HasValue)
            {
                var diff = DateTime.UtcNow - user.LastDriftEmail.Value;
                if (diff < TimeSpan.FromHours(user.EmailCooldownHours))
                    return;
            }

            SendDriftEmail(userEmail, violations);
            user.LastDriftEmail = DateTime.UtcNow;
            _userRepo.Update(user);
        }

        public async Task<Wallet> AddWalletAsync(string address, string network, string userEmail)
        {
            var wal = new Wallet { Address = address, Network = network, Email = userEmail };
            _walletRepo.Create(wal);
            await GetWalletValueUsdAsync(address, network, userEmail);
            RecalculateAllocationPercentages(userEmail);
            CheckAndNotifyDeviation(userEmail);
            return wal;
        }

        public async Task<List<Wallet>> GetWalletsForUserAsync(string email)
        {
            return _walletRepo.Read(w => w.Email == email).ToList();
        }

        public async Task<decimal> GetWalletValueUsdAsync(string address, string network, string userEmail)
        {
            if (network.Equals("solana", StringComparison.OrdinalIgnoreCase))
                return await GetSolanaWalletValueFromTracker(address, userEmail);

            throw new NotSupportedException($"Network {network} not supported yet.");
        }

        public List<CoinHolding> GetHoldingsForWallet(string walletAddress, string walletEmail)
        {
            return _holdingRepo.Read(h => h.WalletAddress == walletAddress && h.WalletEmail == walletEmail).ToList();
        }

        private async Task<decimal> GetSolanaWalletValueFromTracker(string address, string userEmail)
        {
            var client = _http.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://data.solanatracker.io/wallet/{address}");
            request.Headers.Add("x-api-key", _tracker.ApiKey);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;
            if (!root.TryGetProperty("tokens", out var tokens) || !root.TryGetProperty("total", out var total))
                throw new Exception("Ungültige Antwortstruktur von SolanaTracker");

            var wallet = _walletRepo.Read(w => w.Address == address && w.Email == userEmail).FirstOrDefault();
            if (wallet == null)
                throw new Exception("Wallet nicht gefunden für Adresse: " + address + " und Benutzer: " + userEmail);

            _holdingRepo.DeleteRange(
                _holdingRepo.Read(h => h.WalletAddress == address && h.WalletEmail == wallet.Email)
            );

            foreach (var tokenElem in tokens.EnumerateArray())
            {
                if (!tokenElem.TryGetProperty("token", out var tokenInfo)) continue;

                var mint = tokenInfo.GetProperty("mint").GetString();
                if (mint == null) continue;

                var decimals = tokenInfo.TryGetProperty("decimals", out var decProp) ? decProp.GetInt32() : 0;

                var holding = new CoinHolding
                {
                    WalletAddress = address,
                    WalletEmail = wallet.Email,
                    TokenAddress = mint,
                    TokenName = tokenInfo.GetProperty("name").GetString(),
                    Symbol = tokenInfo.GetProperty("symbol").GetString(),
                    ImageUrl = tokenInfo.TryGetProperty("image", out var img) ? img.GetString() : null,
                    Amount = tokenElem.GetProperty("balance").GetDecimal(),
                    UsdValue = tokenElem.GetProperty("value").GetDecimal(),
                    Decimals = decimals
                };

                _holdingRepo.Create(holding);
            }

            wallet.UsdValue = total.GetDecimal();
            wallet.LastUpdated = DateTime.UtcNow;
            _walletRepo.Update(wallet);

            RecalculateAllocationPercentages(wallet.Email);
            CheckAndNotifyDeviation(wallet.Email);

            return total.GetDecimal();
        }

        public async Task RemoveWalletAsync(string address, string userEmail)
        {
            var wallet = _walletRepo.Read(w => w.Address == address && w.Email == userEmail).FirstOrDefault();
            if (wallet != null)
            {
                _walletRepo.Delete(wallet);
                _holdingRepo.DeleteRange(
                    _holdingRepo.Read(h => h.WalletAddress == address && h.WalletEmail == wallet.Email)
                );
                RecalculateAllocationPercentages(userEmail);
                CheckAndNotifyDeviation(userEmail);
            }
        }
    }
}
