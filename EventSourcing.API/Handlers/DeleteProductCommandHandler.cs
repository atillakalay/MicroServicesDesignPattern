using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly ProductStream _productStream;

        public DeleteProductCommandHandler(ProductStream productStream)
        {
            _productStream = productStream ?? throw new ArgumentNullException(nameof(productStream));
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty) throw new ArgumentException("Invalid product id", nameof(request.Id));
            _productStream.Deleted(request.Id);
            await _productStream.SaveAsync().ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
