using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _context;

        private readonly ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.orderId).ConfigureAwait(false);

            if (order == null)
            {
                _logger.LogError($"Order (Id={context.Message.orderId}) not found");
                return;
            }

            order.Status = OrderStatus.Fail;
            order.FailMessage = context.Message.Message;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation($"Order (Id={context.Message.orderId}) status changed: {order.Status}");
        }

    }
}