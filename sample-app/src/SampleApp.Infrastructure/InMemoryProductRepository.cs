using Microsoft.Extensions.Logging;
using SampleApp.Domain;

namespace SampleApp.Infrastructure
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly ILogger<InMemoryProductRepository> logger;
        private readonly object syncRoot = new();
        private readonly List<Product> products;

        public InMemoryProductRepository(ILogger<InMemoryProductRepository> logger)
        {
            this.logger = logger;
            products =
            [
                new Product(
                    Guid.NewGuid(),
                    "Wireless Noise-Cancelling Headphones",
                    "Over-ear headphones with active noise cancellation, 30-hour battery life, and multipoint Bluetooth pairing."),
                new Product(
                    Guid.NewGuid(),
                    "Ergonomic Mechanical Keyboard",
                    "Split mechanical keyboard with tactile switches, adjustable tenting, and USB-C connectivity for long work sessions."),
                new Product(
                    Guid.NewGuid(),
                    "4K USB-C Monitor",
                    "27-inch 4K display with HDR support, 90W power delivery, and integrated USB hub for productivity setups."),
                new Product(
                    Guid.NewGuid(),
                    "Smart Home Security Camera",
                    "Indoor camera with night vision, motion detection alerts, and encrypted cloud recording."),
                new Product(
                    Guid.NewGuid(),
                    "Portable SSD 2TB",
                    "Compact external solid-state drive with USB 3.2 speeds, aluminum casing, and hardware encryption support.")
            ];

            this.logger.LogInformation("In-memory product repository initialized with seeded catalog");
        }

        public Task AddAsync(Product product)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["event.action"] = "Products/Persist",
                ["product.id"] = product.Id,
                ["product.name"] = product.Name
            });

            logger.LogInformation("Persisting product in memory repository");

            lock (syncRoot)
            {
                products.Add(product);
            }

            logger.LogInformation("Product persisted in memory repository");

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            lock (syncRoot)
            {
                return Task.FromResult<IEnumerable<Product>>(products.ToList());
            }
        }

        public Task<Product> GetAsync(Guid productId)
        {
            Product? product;

            lock (syncRoot)
            {
                product = products.FirstOrDefault(current => current.Id == productId);
            }

            if (product is null)
            {
                throw new KeyNotFoundException($"Product with id '{productId}' was not found.");
            }

            return Task.FromResult(product);
        }
    }
}
