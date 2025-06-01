using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PartneroBackend.Models.DTOs;
using PartneroBackend.Models.InfoClasses;
using PartneroBackend.Services;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PartneroBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MicroserviceController : ControllerBase
    {
        [HttpPost]
        [Route("send-business-info")]
        public async Task SendBusinessInfo([FromBody] BusinessInfoForMicroservice businessInfo)
        {
            await MicroserviceHandler.SendBusinessInfo(businessInfo);
        }
    }
}
