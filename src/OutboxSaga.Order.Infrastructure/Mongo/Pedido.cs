namespace OutboxSaga.Order.Infrastructure.Mongo
{
    public class Pedido
    {
        public string Id { get; set; }
        public string ClienteId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
    }
}