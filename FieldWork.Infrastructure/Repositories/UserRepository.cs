
using FieldWork.Application.Authentication;
using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FieldWorkDbContext _db;

    public UserRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users
            .FirstOrDefaultAsync(x => x.Username == username);
    }
}

