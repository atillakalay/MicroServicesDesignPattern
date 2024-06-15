using Shared.Interfaces;

namespace Shared.Events
{
    public class StockReservedRequestPaymentEvent : IStockReservedRequestPayment
    {
        public StockReservedRequestPaymentEvent(Guid correlationId)
        {
            correlationId = CorrelationId;
        }
        public PaymentMessage Payment { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }

        public Guid CorrelationId { get; }
        public string BuyerId { get; set; }
    }
}
