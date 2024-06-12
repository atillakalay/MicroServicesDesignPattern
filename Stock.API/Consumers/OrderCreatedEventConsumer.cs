using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var orderItems = context.Message.orderItems;
            var productIds = orderItems.Select(item => item.ProductId).ToList();

            var stocks = await _appDbContext.Stocks
                .Where(stock => productIds.Contains(stock.ProductId))
                .ToListAsync();

            bool hasSufficientStock = orderItems.All(item =>
                stocks.Any(stock => stock.ProductId == item.ProductId && stock.Count >= item.Count));

            if (hasSufficientStock)
            {
                foreach (var item in orderItems)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }
                }
                await _appDbContext.SaveChangesAsync();

                _logger.LogInformation($"Stock was reserved for Buyer Id:{context.Message.BuyerId}");

                ISendEndpoint sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));
                StockReservedEvent stockReservedEvent = new()
                {
                    Payment = context.Message.Payment,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.orderItems
                };

                await sendEndPoint.Send(stockReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Not enough stock!"
                });
                _logger.LogInformation($"Not enough stock for Buyer Id: {context.Message.BuyerId}");
            }
        }

    }
}
