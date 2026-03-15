namespace SampleApp.WebAPI.Controllers
{
    public class CreateProductRequest
    {
        public required string Name { get; init; }

        public required string Description { get; init; }
    }
}
