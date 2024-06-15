using Shared.Interfaces;

namespace Shared
{
    public class StockNotReservedEvent : IStockNotReservedEvent
    {
        public StockNotReservedEvent(Guid correlationId)
        {
            correlationId = CorrelationId;
        }
        public string Reason { get; set; }

        public Guid CorrelationId { get; }
    }
}
