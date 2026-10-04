using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutboxSaga.Infrastructure.Outbox
{
    public class CosmosOutboxRepository : IOutboxRepository
    {
        // Cosmos DB client and context would be injected here
        public Task AddAsync(OutboxMessage message)
        {
            // Implement Cosmos DB insert logic
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync()
        {
            // Implement Cosmos DB query logic
            throw new NotImplementedException();
        }

        public Task MarkAsProcessedAsync(string id)
        {
            // Implement Cosmos DB update logic
            throw new NotImplementedException();
        }
    }
}