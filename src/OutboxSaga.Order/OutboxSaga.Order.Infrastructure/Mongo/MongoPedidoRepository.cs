using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutboxSaga.Order.Infrastructure.Mongo
{
    public class MongoPedidoRepository : IPedidoRepository
    {
        // MongoDB client/context seria injetado aqui
        public Task AddAsync(Pedido pedido)
        {
            // Implementar lógica de inserção no MongoDB
            throw new NotImplementedException();
        }

        public Task<Pedido> GetByIdAsync(string id)
        {
            // Implementar lógica de busca por ID no MongoDB
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Pedido>> GetAllAsync()
        {
            // Implementar lógica de busca de todos os pedidos
            throw new NotImplementedException();
        }

        public Task UpdateStatusAsync(string id, string status)
        {
            // Implementar lógica de atualização de status
            throw new NotImplementedException();
        }
    }
}