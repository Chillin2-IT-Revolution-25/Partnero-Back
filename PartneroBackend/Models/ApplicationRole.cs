using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace PartneroBackend.Models
{
    [CollectionName("roles")]
    public class ApplicationRole : MongoIdentityRole<ObjectId>
    {
    }
}
