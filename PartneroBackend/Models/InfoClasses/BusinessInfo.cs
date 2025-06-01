using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartneroBackend.Models.InfoClasses
{
    public enum CompanySize
    {
        Small,
        Medium,
        Large
    }

    public class BusinessInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        public string BusinessName { get; set; }
        public string Category { get; set; }
        public LocationInfo? Location { get; set; }

        public string? Description { get; set; }
        public CompanySize? CompanySize { get; set; }
        public int? FoundedYear { get; set; }

        public List<string> BusinessImageUrls { get; set; } = [];
        public SocialMediaInfo SocialMedia { get; set; } = new();
    }
}
