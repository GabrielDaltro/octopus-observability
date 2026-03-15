using Microsoft.Extensions.Logging;
using SampleApp.Domain;

namespace SampleApp.Application
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository productRepository;
        private readonly ILogger<ProductService> logger;

        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            this.productRepository = productRepository;
            this.logger = logger;
        }

        public async Task<Product> CreateProductAsync(string name, string description)
        {
            using var createScope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["event.action"] = "Products/Create",
                ["product.name"] = name
            });

            logger.LogInformation("Starting product creation flow");

            var product = Product.Create(name, description);

            using var productScope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["product.id"] = product.Id
            });

            await productRepository.AddAsync(product);

            logger.LogInformation("Product entity created");

            return product;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            using var scope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["event.action"] = "Products/List"
            });

            logger.LogInformation("Loading products");

            var products = (await productRepository.GetAllAsync()).ToList();

            logger.LogInformation("Products loaded");

            return products;
        }
    }
}
