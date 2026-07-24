using WebHookDeliveryService.Domain.Models;

namespace WebHookDeliveryService.Domain.Interfaces;

public interface IDeadLetterRepository
{
    Task<DeadLetter?> GetByIdAsync(Guid id);
    Task<List<DeadLetter>> GetAllAsync(int skip = 0, int take = 50);
    Task<DeadLetter> CreateAsync(DeadLetter deadLetter);
    Task DeleteAsync(Guid id);
    Task<int> GetTotalCountAsync();
    Task<int> CleanupExpiredAsync(DateTime now);
}
