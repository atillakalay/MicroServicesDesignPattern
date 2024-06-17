using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers
{
    public class ChangeProductNameCommandHandler : IRequestHandler<ChangeProductNameCommand>
    {
        private readonly ProductStream _productStream;

        public ChangeProductNameCommandHandler(ProductStream productStream)
        {
            _productStream = productStream ?? throw new ArgumentNullException(nameof(productStream));
        }

        public async Task<Unit> Handle(ChangeProductNameCommand request, CancellationToken cancellationToken)
        {
            if (request.ChangeProductNameDto == null) throw new ArgumentNullException(nameof(request.ChangeProductNameDto));

            _productStream.NameChanged(request.ChangeProductNameDto);

            await _productStream.SaveAsync().ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
