using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Products.Service.Models;

namespace Products.Service.Services
{
    public class SystemHistoryService
    {

        private readonly IMongoCollection<SystemHistory> _systemHistoryCollection;

        public SystemHistoryService(IOptions<MongoContext> mongoContext)
        {
            var mongoClient = new MongoClient(
            mongoContext.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoContext.Value.Database);

            _systemHistoryCollection = mongoDatabase.GetCollection<SystemHistory>(
                mongoContext.Value.Collection);
        }

        public async Task<List<SystemHistory>> GetAsync()
        {
            var projectionDefinition = Builders<SystemHistory>.Projection.Exclude("_id");
            return await _systemHistoryCollection.Find(_ => true)
                                                 .Project<SystemHistory>(projectionDefinition)
                                                 .ToListAsync();
        }

        public async Task CreateAsync(SystemHistory systemHistory)
        {
            await _systemHistoryCollection.InsertOneAsync(systemHistory);
        }

    }

    public class SystemHistory
    {
        public string action { get; set; }
        public object actor { get; set; }
        public string table { get; set; }
        public DateTime time { get; set; }
        public object value { get; set; }
    }
}
