using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
}
