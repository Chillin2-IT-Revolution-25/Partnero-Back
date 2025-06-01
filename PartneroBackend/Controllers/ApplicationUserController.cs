using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PartneroBackend.Data;
using PartneroBackend.Models.DTOs;
using PartneroBackend.Models.InfoClasses;
using PartneroBackend.Services;

namespace PartneroBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController(MongoDbContext context) : ControllerBase
    {
        [HttpGet]
        [Route("business/{id}")]
        public async Task<BusinessInfo?> GetBusinessById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }

            var business = await context.BusinessInfos.Find(b => b.Id == objectId).FirstOrDefaultAsync();

            return business;
        }
    }
}
