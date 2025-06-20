// UtilityBill.Data/Repositories/IUserRepository.cs
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}