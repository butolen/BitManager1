using Domain.interfaces;
using Domain.repositories;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Model;
using MudBlazor.Services;
using PortfolioApp.Enities;
using Solnet.Rpc;
using WebGUI.Data;
using WebGUI.Processses;
using WebGUI.Services;
using WebGUI.Settings;

var builder = WebApplication.CreateBuilder(args);

// ========== 🔧 SETTINGS ==========
builder.Services.Configure<SolanaTrackerSettings>(builder.Configuration.GetSection("SolanaTracker"));

// ========== 🔧 DB CONTEXT ==========
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// ========== 📦 REPOSITORIES ==========
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<ChainRepository>();

builder.Services.AddScoped<IRepository<Chain>, ChainRepository>();
builder.Services.AddScoped<IRepository<CoinHolding>, CoinHoldingRepository>();
builder.Services.AddScoped<IRepository<Wallet>, WalletRepository>();
builder.Services.AddScoped<IRepository<TargetAllocation>, TargetAllocationRepository>();
builder.Services.AddScoped<IRepository<RebalanceSession>, RebalanceSessionRepository>();
builder.Services.AddScoped<IRepository<RebalanceSwap>, RebalanceSwapRepository>();
builder.Services.AddScoped<IRepository<PendingUser>, PendingUserRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();

// ========== ⚙️ SERVICES ==========

builder.Services.AddScoped<TargetAllocationService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RebalanceService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<WalletUpdateBackgroundService>();
builder.Services.AddSingleton<IRpcClient>(_ => ClientFactory.GetClient(Cluster.MainNet));

// ========== 🔐 AUTH ==========
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/user/login";
        options.LogoutPath = "/user/logout";
    });
builder.Services.AddAuthorization();

// ========== 🌐 UI ==========
builder.Services.AddMudServices();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// ========== 🏁 BUILD ==========
var app = builder.Build();


// ========== 🔧 MIDDLEWARE ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();