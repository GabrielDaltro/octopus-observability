using Microsoft.AspNetCore.Mvc;
using SampleApp.Application;
using SampleApp.Domain;

namespace SampleApp.WebAPI.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly ILogger<ProductController> logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            this.productService = productService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
           return Ok(await productService.GetProducts());
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductRequest request)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["event.action"] = "Products/Create",
                ["product.name"] = request.Name
            });

            logger.LogInformation("Received create product request");

            var product = await productService.CreateProductAsync(request.Name, request.Description);

            return Created($"/products/{product.Id}", product);
        }
    }
}
