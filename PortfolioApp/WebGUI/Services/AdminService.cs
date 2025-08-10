using Domain.interfaces;
using Model.Entities;
using PortfolioApp.Enities;

namespace WebGUI.Services;
public interface IAdminService
{
    IEnumerable<User> GetAllUsersExceptAdmins();
    User? GetUserByEmail(string email);
    void DeleteUser(string email);
}

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepo;

    public AdminService(IRepository<User> userRepo)
    {
        _userRepo = userRepo;
    }

    public IEnumerable<User> GetAllUsersExceptAdmins()
        => _userRepo.Read(u => u.Role != Role.Admin);

    public User? GetUserByEmail(string email)
        => _userRepo.Read(u => u.Email == email).FirstOrDefault();

    public void DeleteUser(string email)
    {
        var user = GetUserByEmail(email);
        if (user != null && user.Role != Role.Admin)
        {
            _userRepo.Delete(user);
        }
    }
}