﻿using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly AppDbContext _context;

        private readonly ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentCompletedEventConsumer(AppDbContext context, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId).ConfigureAwait(false);

            if (order == null)
            {
                _logger.LogError($"Order (Id={context.Message.OrderId}) not found");
                return;
            }

            order.Status = OrderStatus.Complete;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed: {order.Status}");
        }

    }
}