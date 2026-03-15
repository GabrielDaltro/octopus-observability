namespace SampleApp.Domain
{
    public class Product : Entity
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public static Product Create(string name, string description)
        {
            return new Product(Guid.NewGuid(), name, description);
        }

        public Product(Guid id, string name, string description) : base(id)
        {
            Name = name;
            Description = description;
        }
    }
}
