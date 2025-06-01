using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using PartneroBackend.Models.InfoClasses;

namespace PartneroBackend.Models.DTOs
{
    public class BusinessInfoForMicroservice
    {
        public string name { get; set; }
        public string? description { get; set; }
        public string category { get; set; }
        public LocationChangeForDamian? location { get; set; }

        public int company_size { get; set; }
        public int? founded_year { get; set; }

        public List<string> business_image_urls { get; set; } = [];
        public SocialMediaInfo social_media { get; set; } = new();
        public string user_id { get; set; }
    }

    public class LocationChangeForDamian
    {
        public string display_name { get; set; }

        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }

        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
