namespace WebGUI.Services;
// Domain/interfaces/IWalletService.cs
using PortfolioApp.Enities;

public interface IWalletService
{
    public List<CoinHolding> GetHoldingsForWallet(string walletAddress, string userEmail);
    Task<Wallet> AddWalletAsync(string address, string network, string userEmail);
    Task<List<Wallet>> GetWalletsForUserAsync(string email);
    Task<decimal> GetWalletValueUsdAsync(string address, string network, string userEmail);
    Task RemoveWalletAsync(string address, string userEmail);
    void CheckAndNotifyDeviation(string userEmail);
}