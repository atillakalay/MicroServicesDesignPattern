using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using static Order.API.Models.Order;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext appDbContext, IPublishEndpoint publishEndpoint)
        {
            _context = appDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
        {
            var newOrder = MapToOrder(orderCreateDto);

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            var orderCreatedEvent = CreateOrderCreatedEvent(orderCreateDto, newOrder);

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }

        private Models.Order MapToOrder(OrderCreateDto orderCreateDto)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreateDto.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address
                {
                    Line = orderCreateDto.Address.Line,
                    Province = orderCreateDto.Address.Province,
                    District = orderCreateDto.Address.District
                },
                CreatedDate = DateTime.Now,
                Items = orderCreateDto.orderItems.Select(item => new OrderItem
                {
                    Price = item.Price,
                    ProductId = item.ProductId,
                    Count = item.Count
                }).ToList(),
                FailMessage = string.Empty
            };
            return newOrder;
        }

        private OrderCreatedEvent CreateOrderCreatedEvent(OrderCreateDto orderCreateDto, Models.Order newOrder)
        {
            var orderCreatedEvent = new OrderCreatedEvent
            {
                BuyerId = orderCreateDto.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreateDto.Payment.CardName,
                    CardNumber = orderCreateDto.Payment.CardNumber,
                    Expiration = orderCreateDto.Payment.Expiration,
                    CVV = orderCreateDto.Payment.CVV,
                    TotalPrice = orderCreateDto.orderItems.Sum(x => x.Price * x.Count),
                },
                orderItems = orderCreateDto.orderItems.Select(item => new OrderItemMessage
                {
                    Count = item.Count,
                    ProductId = item.ProductId
                }).ToList()
            };
            return orderCreatedEvent;
        }
    }
}
