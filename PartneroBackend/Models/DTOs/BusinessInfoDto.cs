using Amazon.Runtime.EventStreams;
using MongoDB.Bson;

namespace PartneroBackend.Models.DTOs
{
    public class BusinessInfoDto
    {
        public ObjectId Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public List<string> Offers { get; set; } = [];
    }
}
