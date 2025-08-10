using Domain.interfaces;
using PortfolioApp.Enities;
using WebGUI.Services;

namespace WebGUI.Processses;

public class WalletUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public WalletUpdateBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
            var walletRepo = scope.ServiceProvider.GetRequiredService<IRepository<Wallet>>();

            var allWallets = walletRepo.ReadAll();

            foreach (var wallet in allWallets)
            {
                try
                {
                    await walletService.GetWalletValueUsdAsync(wallet.Address, wallet.Network, wallet.Email);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Aktualisieren der Wallet {wallet.Address}: {ex.Message}");
                }
                walletService.CheckAndNotifyDeviation(wallet.Email);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}