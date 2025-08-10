using System.Net;
using System.Net.Mail;
using Domain.interfaces;
using Domain.Repositories;
using Model.Entities;
using PortfolioApp.Enities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace WebGUI.Services;

public class UserService : IUserService
{
    private readonly IRepository<PendingUser> _pendingRepo;
    private readonly IRepository<User> _userRepo;

    public UserService(IRepository<User> userRepo, IRepository<PendingUser> pendingRepo)
    {
        _userRepo = userRepo;
        _pendingRepo = pendingRepo;
    }

    public User? GetByEmail(string email)
        => _userRepo.Read(u => u.Email == email).FirstOrDefault();

    public void Update(User user)
        => _userRepo.Update(user);

    public async Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword)
    {
        var user = _userRepo.Read(u => u.PasswordResetToken == token).FirstOrDefault();
        if (user == null)
            return false;

        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        _userRepo.Update(user);
        return true;
    }

    public bool ChangeEmail(string oldEmail, string newEmail)
    {
        if (_userRepo.Read(u => u.Email == newEmail).Any())
            return false;

        var user = _userRepo.Read(u => u.Email == oldEmail).FirstOrDefault();
        if (user == null)
            return false;

        user.Email = newEmail;
        _userRepo.Update(user);
        return true;
    }

    public async Task<bool> SendResetPasswordEmailAsync(string email)
    {
        var user = _userRepo.Read(u => u.Email == email).FirstOrDefault();
        if (user == null)
            return false;

        string token = Guid.NewGuid().ToString();
        user.PasswordResetToken = token;
        _userRepo.Update(user);

        SendResetPasswordEmail(email, token);
        return true;
    }

    private void SendVerificationEmail(string to, string subject, string htmlBody)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress("mathiasbutolen@gmail.com");
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = htmlBody;
        mail.IsBodyHtml = true;

        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("mathiasbutolen@gmail.com", "tlzbwhyawsugzqlc"),
            EnableSsl = true
        };

        smtp.Send(mail);
    }

    public async Task<bool> RegisterStartAsync(string email, string password)
    {
        var existing = _userRepo.Read(u => u.Email == email).FirstOrDefault();
        if (existing != null)
            return false;

        string token = Guid.NewGuid().ToString();

        var pendingUser = new PendingUser
        {
            Email = email,
            PlainPassword = password,
            Token = token,
            CreatedAt = DateTime.UtcNow
        };

        _pendingRepo.Create(pendingUser);

        string verifyUrl = $"http://localhost:5281/user/verify?token={Uri.EscapeDataString(token)}";
        string mailText = $@"
<!DOCTYPE html>
<html lang='en'>
<head><meta charset='UTF-8'><title>Account Verification</title></head>
<body style='background:#f8f9fa;font-family:Arial,sans-serif;'>
<div style='max-width:600px;margin:40px auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 0 10px rgba(0,0,0,0.1);'>
    <div style='text-align:center;margin-bottom:30px;'>
        <img src='http://localhost:5281/images/logo.png' alt='BitManager Logo' style='height:300px;' />
    </div>
    <h2 style='text-align:center;color:#343a40;'>Confirm your email</h2>
    <p style='text-align:center;color:#495057;'>Thank you for registering with <strong>BitManager</strong>.</p>
    <div style='text-align:center;margin:30px 0;'>
        <a href='{verifyUrl}' style='background:#0d6efd;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:bold;'>Verify Email</a>
    </div>
    <p style='text-align:center;font-size:14px;color:#6c757d;'>If you didn’t create this account, you can ignore this email.</p>
    <hr style='margin:40px 0;border-top:1px solid #dee2e6;' />
    <p style='text-align:center;font-size:12px;color:#adb5bd;'>&copy; 2025 BitManager Inc. All rights reserved.</p>
</div>
</body>
</html>";

        SendVerificationEmail(email, "Bitte bestätige deine E-Mail-Adresse", mailText);
        return true;
    }

    private void SendResetPasswordEmail(string to, string token)
    {
        string resetUrl = $"http://localhost:5281/user/reset-password?token={Uri.EscapeDataString(token)}";
        string htmlBody = $@"
<!DOCTYPE html>
<html lang='en'>
<head><meta charset='UTF-8' /></head>
<body style='background:#f8f9fa;font-family:Arial,sans-serif;'>
<div style='max-width:600px;margin:40px auto;background:#fff;padding:40px;border-radius:12px;box-shadow:0 0 12px rgba(0,0,0,0.08);text-align:center;'>
    <img src='http://localhost:5281/images/logo.png' alt='BitManager Logo' style='margin-bottom:20px;height:300px;' />
    <h2 style='color:#212529;'>Reset Your Password</h2>
    <p style='color:#495057;'>You requested a password reset.</p>
    <a href='{resetUrl}' style='display:inline-block;background:#0d6efd;color:white;padding:12px 24px;text-decoration:none;border-radius:8px;margin-top:20px;font-weight:bold;'>Reset Password</a>
    <p style='margin-top:30px;font-size:14px;color:#6c757d;'>If you didn’t request this, just ignore this email.</p>
    <hr style='margin:40px 0;border-top:1px solid #dee2e6;' />
    <footer style='font-size:13px;color:#adb5bd;'>&copy; 2025 BitManager Inc. All rights reserved.</footer>
</div>
</body>
</html>";

        var mail = new MailMessage("mathiasbutolen@gmail.com", to, "Password Reset", htmlBody)
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

    private void SendTwoFactorCodeEmail(string to, string code)
    {
        string htmlBody = $@"
        <h3>Your verification code</h3>
        <p>Please enter the following 4-digit code to complete your login:</p>
        <div style='font-size:24px; font-weight:bold; margin:20px 0;'>{code}</div>
        <p>This code is valid for 10 minutes.</p>";

        var mail = new MailMessage("mathiasbutolen@gmail.com", to, "Your 2FA Code", htmlBody)
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

    public async Task<bool> LoginAsync(string email, string password, HttpContext httpContext)
    {
        var user = _userRepo.Read(u => u.Email == email).FirstOrDefault();
        if (user == null || !user.EmailConfirmed)
            return false;

        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.HashedPassword);
        if (!isValid)
            return false;

        if (user.TwoFactorEnabled)
        {
            var random = new Random();
            string code = random.Next(1000, 9999).ToString();

            user.TwoFactorCode = code;
            user.TwoFactorExpiresAt = DateTime.UtcNow.AddMinutes(10);
            user.TwoFactorTempToken = Guid.NewGuid().ToString(); 
            _userRepo.Update(user);

            SendTwoFactorCodeEmail(user.Email, code);
            return false;
        }

        await SignInAsync(user, httpContext);
        return true;
    }

    public async Task SignInAsync(User user, HttpContext httpContext)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    public async Task LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public string? CompleteEmailVerification(string token)
    {
        var pending = _pendingRepo.Read(p => p.Token == token).FirstOrDefault();
        if (pending == null || (DateTime.UtcNow - pending.CreatedAt).TotalHours > 24)
            return null;

        var user = new User
        {
            Email = pending.Email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(pending.PlainPassword),
            Role = Role.User,
            EmailConfirmed = true
        };

        _userRepo.Create(user);
        _pendingRepo.Delete(pending);

        return user.Email + "|" + pending.PlainPassword;
    }
}