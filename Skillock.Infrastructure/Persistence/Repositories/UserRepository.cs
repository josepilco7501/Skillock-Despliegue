using Microsoft.EntityFrameworkCore;
using Skillock.Application.Interfaces;
using Skillock.Domain.Interfaces;
using Skillock.Domain.Models;
using Skillock.Infrastructure.Context;

namespace Skillock.Infrastructure.Persistence.Repositories;

public class UserRepository(SkillockDbContext context) : Repository<User>(context), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => Context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> AnyByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> AnyByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => Context.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public Task<bool> AnyByRoleAsync(string role, CancellationToken cancellationToken = default)
        => Context.Users.AnyAsync(u => u.Role == role, cancellationToken);
}
