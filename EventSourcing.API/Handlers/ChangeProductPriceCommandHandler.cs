using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers
{
    public class ChangeProductPriceCommandHandler : IRequestHandler<ChangeProductPriceCommand>
    {
        private readonly ProductStream _productStream;

        public ChangeProductPriceCommandHandler(ProductStream productStream)
        {
            _productStream = productStream ?? throw new ArgumentNullException(nameof(productStream));
        }

        public async Task<Unit> Handle(ChangeProductPriceCommand request, CancellationToken cancellationToken)
        {
            if (request.ChangeProductPriceDto == null) throw new ArgumentNullException(nameof(request.ChangeProductPriceDto));
            _productStream.PriceChanged(request.ChangeProductPriceDto);
            await _productStream.SaveAsync().ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
