// UtilityBill.Data/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(UtilityBillDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
    }
}