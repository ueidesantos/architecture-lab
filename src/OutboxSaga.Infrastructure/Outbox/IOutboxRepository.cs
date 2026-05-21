using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutboxSaga.Infrastructure.Outbox
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
        Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync();
        Task MarkAsProcessedAsync(string id);
    }
}