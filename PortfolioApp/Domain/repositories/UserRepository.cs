using Model.Entities;

using Domain.repositories;
using Model;
using PortfolioApp.Enities;

namespace Domain.Repositories;

public class UserRepository : ARepository<User>
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

   
}