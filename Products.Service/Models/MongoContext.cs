namespace Products.Service.Models
{
    public class MongoContext
    {

        public string ConnectionString { get; set; } = null!;

        public string Database { get; set; } = null!;

        public string Collection { get; set; } = null!;

    }
}
