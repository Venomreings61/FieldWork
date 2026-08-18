
using FieldWork.Domain.Entities;

namespace FieldWork.Application.Authentication;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
}

