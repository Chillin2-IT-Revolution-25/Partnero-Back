using MongoDB.Driver;
using PartneroBackend.Models.InfoClasses;

namespace PartneroBackend.Data
{
    public class MongoDbContext
    {
        public IMongoCollection<BusinessInfo> BusinessInfos { get; }

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDbConnectionString"));
            var database = client.GetDatabase("Partnero");

            BusinessInfos = database.GetCollection<BusinessInfo>("BusinessInfos");

        }
    }
}
