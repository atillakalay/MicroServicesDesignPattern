using EventSourcing.API.DTOs;
using EventSourcing.Shared.Events;
using EventStore.ClientAPI;

namespace EventSourcing.API.EventStores
{
    public class ProductStream : AbstractStream
    {
        public static string StreamName => "ProductStream";
        public static string GroupName => "agroup";

        public ProductStream(IEventStoreConnection eventStoreConnection) : base(StreamName, eventStoreConnection)
        {
        }

        public void Created(CreateProductDto createProductDto)
        {
            if (createProductDto is null) throw new ArgumentNullException(nameof(createProductDto));

            Events.Add(new ProductCreatedEvent
            {
                Id = Guid.NewGuid(),
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock,
                UserId = createProductDto.UserId
            });
        }

        public void NameChanged(ChangeProductNameDto changeProductNameDto)
        {
            if (changeProductNameDto is null) throw new ArgumentNullException(nameof(changeProductNameDto));

            Events.Add(new ProductNameChangedEvent
            {
                ChangedName = changeProductNameDto.Name,
                Id = changeProductNameDto.Id
            });
        }

        public void PriceChanged(ChangeProductPriceDto changeProductPriceDto)
        {
            if (changeProductPriceDto is null) throw new ArgumentNullException(nameof(changeProductPriceDto));

            Events.Add(new ProductPriceChangedEvent
            {
                ChangedPrice = changeProductPriceDto.Price,
                Id = changeProductPriceDto.Id
            });
        }

        public void Deleted(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid id", nameof(id));

            Events.Add(new ProductDeletedEvent
            {
                Id = id
            });
        }
    }
}
