using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;
using OrderCreatedEvent = Shared.Events.OrderCreatedEvent;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        // Events
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }

        // States
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State StockNotReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }

        public OrderStateMachine()
        {
            ConfigureInstanceState();
            ConfigureEvents();
            ConfigureStateMachine();
        }

        private void ConfigureInstanceState()
        {
            InstanceState(x => x.CurrentState);
        }

        private void ConfigureEvents()
        {
            Event(() => OrderCreatedRequestEvent, config =>
                config.CorrelateBy<int>(x => x.OrderId, context => context.Message.OrderId)
                      .SelectId(context => Guid.NewGuid()));

            Event(() => StockReservedEvent, config =>
                config.CorrelateById(context => context.Message.CorrelationId));

            Event(() => StockNotReservedEvent, config =>
                config.CorrelateById(context => context.Message.CorrelationId));

            Event(() => PaymentCompletedEvent, config =>
                config.CorrelateById(context => context.Message.CorrelationId));

            Event(() => PaymentFailedEvent, config =>
                config.CorrelateById(context => context.Message.CorrelationId));
        }

        private void ConfigureStateMachine()
        {
            Initially(
                When(OrderCreatedRequestEvent)
                    .Then(HandleOrderCreatedRequestEvent)
                    .TransitionTo(OrderCreated)
            );

            During(OrderCreated,
                When(StockReservedEvent)
                    .Then(HandleStockReservedEvent)
                    .TransitionTo(StockReserved),

                When(StockNotReservedEvent)
                    .Then(HandleStockNotReservedEvent)
                    .TransitionTo(StockNotReserved)
            );

            During(StockReserved,
                When(PaymentCompletedEvent)
                    .Then(HandlePaymentCompletedEvent)
                    .TransitionTo(PaymentCompleted)
                    .Finalize(),

                When(PaymentFailedEvent)
                    .Then(HandlePaymentFailedEvent)
                    .TransitionTo(PaymentFailed)
            );
        }

        // Event Handlers
        private void HandleOrderCreatedRequestEvent(BehaviorContext<OrderStateInstance, IOrderCreatedRequestEvent> context)
        {
            var saga = context.Saga;
            var message = context.Message;

            saga.BuyerId = message.BuyerId;
            saga.OrderId = message.OrderId;
            saga.CreatedDate = DateTime.Now;

            saga.CardName = message.Payment.CardName;
            saga.CardNumber = message.Payment.CardNumber;
            saga.CVV = message.Payment.CVV;
            saga.Expiration = message.Payment.Expiration;
            saga.TotalPrice = message.Payment.TotalPrice;

            Console.WriteLine($"OrderCreatedRequestEvent before : {saga}");

            context.Publish(new OrderCreatedEvent(saga.CorrelationId)
            {
                OrderItems = message.OrderItems
            });

            Console.WriteLine($"OrderCreatedRequestEvent After : {saga}");
        }

        private void HandleStockReservedEvent(BehaviorContext<OrderStateInstance, IStockReservedEvent> context)
        {
            var saga = context.Saga;

            context.Send(new Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"), new StockReservedRequestPaymentEvent(saga.CorrelationId)
            {
                OrderItems = context.Message.OrderItems,
                Payment = new PaymentMessage
                {
                    CardName = saga.CardName,
                    CardNumber = saga.CardNumber,
                    CVV = saga.CVV,
                    Expiration = saga.Expiration,
                    TotalPrice = saga.TotalPrice
                },
                BuyerId = saga.BuyerId
            });

            Console.WriteLine($"StockReservedEvent After : {saga}");
        }

        private void HandleStockNotReservedEvent(BehaviorContext<OrderStateInstance, IStockNotReservedEvent> context)
        {
            var saga = context.Saga;

            context.Publish(new OrderRequestFailedEvent
            {
                OrderId = saga.OrderId,
                Reason = context.Message.Reason
            });

            Console.WriteLine($"StockNotReservedEvent After : {saga}");
        }

        private void HandlePaymentCompletedEvent(BehaviorContext<OrderStateInstance, IPaymentCompletedEvent> context)
        {
            var saga = context.Saga;

            context.Publish(new OrderRequestCompletedEvent
            {
                OrderId = saga.OrderId
            });

            Console.WriteLine($"PaymentCompletedEvent After : {saga}");
        }

        private void HandlePaymentFailedEvent(BehaviorContext<OrderStateInstance, IPaymentFailedEvent> context)
        {
            var saga = context.Saga;

            context.Publish(new OrderRequestFailedEvent
            {
                OrderId = saga.OrderId,
                Reason = context.Message.Reason
            });

            context.Send(new Uri($"queue:{RabbitMQSettingsConst.StockRollBackMessageQueueName}"), new StockRollbackMessage
            {
                OrderItems = context.Message.OrderItems
            });

            Console.WriteLine($"PaymentFailedEvent After : {saga}");
        }
    }
}
