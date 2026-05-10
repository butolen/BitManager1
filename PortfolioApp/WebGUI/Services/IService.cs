using PortfolioApp.Enities;

namespace WebGUI.Services;

using System.Security.Claims;

public interface IUserService
{
   
    Task<bool> LoginAsync(string email, string password, HttpContext httpContext);
    Task LogoutAsync(HttpContext httpContext);
    Task<bool> RegisterStartAsync(string email, string password);
    string? CompleteEmailVerification(string token);
    Task<bool> SendResetPasswordEmailAsync(string email);
    Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword);
    public User? GetByEmail(string email);
    public void Update(User user);
    public bool ChangeEmail(string oldEmail, string newEmail);
    Task SignInAsync(User user, HttpContext httpContext);

}