using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Application.Common.Interfaces;

public interface IFeedbackFieldDefinitionRepository
{
    Task<IReadOnlyList<FeedbackFieldDefinition>> ListByAppAsync(Guid feedbackAppId, CancellationToken ct = default);
    Task<FeedbackFieldDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateAsync(FeedbackFieldDefinition definition, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
