using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutboxSaga.Order.Infrastructure.Mongo
{
    public interface IPedidoRepository
    {
        Task AddAsync(Pedido pedido);
        Task<Pedido> GetByIdAsync(string id);
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task UpdateStatusAsync(string id, string status);
    }
}