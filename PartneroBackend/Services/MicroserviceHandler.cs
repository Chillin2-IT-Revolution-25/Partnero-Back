using MongoDB.Bson;
using PartneroBackend.Models.DTOs;
using PartneroBackend.Models.InfoClasses;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PartneroBackend.Services
{
    public static class MicroserviceHandler
    {
        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<List<string>> FetchOffersForBusiness(ObjectId businessId)
        {
            try
            {
                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync($"http://localhost:8000/api/offers/{businessId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var offers = JsonSerializer.Deserialize<List<string>>(json, CachedJsonSerializerOptions);

                    return offers ?? [];
                }

                return [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching offers for business {businessId}: {ex.Message}");
                return [];
            }
        }

        public static async Task<string?> SendBusinessInfo(BusinessInfoForMicroservice businessInfo)
        {
            ServicePointManager.Expect100Continue = false;

            var handler = new HttpClientHandler();
            using var httpClient = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version11
            };

            var json = JsonSerializer.Serialize(businessInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://bdf5-91-201-188-236.ngrok-free.app/api/business")
            {
                Version = HttpVersion.Version11,
                Content = content
            };

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonSerializer.Deserialize<BusinessResponse>(responseJson);
                Console.WriteLine("Data sent successfully. ID: " + doc?.Id);
                return doc?.Id;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to send data: {response.StatusCode}\n{error}");
                return null;
            }
        }

        public class BusinessResponse
        {
            public string Id { get; set; }
        }
    }
}
