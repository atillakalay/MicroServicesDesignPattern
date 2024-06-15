using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Interfaces;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger,
            IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            List<bool> stockResult = new();

            foreach (var item in context.Message.OrderItems)
            {
                bool isStockAvailable = await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count >= item.Count);
                stockResult.Add(isStockAvailable);

                if (!isStockAvailable)
                {
                    await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                    {
                        Reason = "Not enough stock"
                    });

                    _logger.LogInformation($"Not enough stock for CorrelationId Id :{context.Message.CorrelationId}");
                    return;
                }
            }

            foreach (var item in context.Message.OrderItems)
            {
                Models.Stock? stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
                if (stock != null)
                {
                    stock.Count -= item.Count;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Stock was reserved for CorrelationId Id :{context.Message.CorrelationId}");

            StockReservedEvent stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
            {
                OrderItems = context.Message.OrderItems
            };

            await _publishEndpoint.Publish(stockReservedEvent);
        }
    }
}
