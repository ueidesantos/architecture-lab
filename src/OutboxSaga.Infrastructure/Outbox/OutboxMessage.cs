namespace OutboxSaga.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}