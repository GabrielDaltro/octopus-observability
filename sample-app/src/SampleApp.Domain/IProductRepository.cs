namespace SampleApp.Domain
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);

        Task<Product> GetAsync(Guid productId);

        Task<IEnumerable<Product>> GetAllAsync();
    }
}