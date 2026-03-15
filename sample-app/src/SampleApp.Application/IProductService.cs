using SampleApp.Domain;

namespace SampleApp.Application
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(string name, string description);

        Task<IEnumerable<Product>> GetProducts();
    }
}
