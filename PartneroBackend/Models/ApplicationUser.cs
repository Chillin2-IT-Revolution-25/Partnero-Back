using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using PartneroBackend.Models.InfoClasses;

namespace PartneroBackend.Models
{
    [CollectionName("users")]
    public class ApplicationUser : MongoIdentityUser<ObjectId>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BusinessInfoId { get; set; }

        [BsonIgnore]
        public BusinessInfo? BusinessInfo { get; set; }

        public PersonalInfo PersonalInfo { get; set; } = new();
        public bool ShowBusinessProfile { get; set; } = false;
    }
}
