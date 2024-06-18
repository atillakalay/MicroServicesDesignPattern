using ServiceA.API.Models;

namespace ServiceA.API
{
    public class ProductService
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductService> _logger;

        public ProductService(HttpClient client, ILogger<ProductService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            try
            {
                var product = await _client.GetFromJsonAsync<Product>($"api/products/{id}");
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    return null;
                }

                _logger.LogInformation("Product retrieved: {Id} - {Name}", product.Id, product.Name);
                return product;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error retrieving product with ID {id}");
                throw;
            }
        }
    }
}
