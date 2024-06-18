using Microsoft.AspNetCore.Mvc;
using ServiceA.API.Models;

namespace ServiceA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Product? product = await _productService.GetProductAsync(id);
            return Ok(product);
        }
    }
}
