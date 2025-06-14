﻿namespace PartneroBackend.Models.InfoClasses
{
    public class LocationInfo
    {
        public string DisplayName { get; set; }

        public string Street { get; set; }          
        public string City { get; set; }             
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
