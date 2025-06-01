using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using PartneroBackend.Data;
using PartneroBackend.Models;
using PartneroBackend.Models.DTOs;
using PartneroBackend.Models.DTOs.AuthDTOs;
using PartneroBackend.Models.InfoClasses;
using PartneroBackend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace PartneroBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager, 
        IConfiguration configuration, 
        MongoDbContext context) : ControllerBase
    {
        [HttpPost]
        [Route("roles/create")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleName)
        {
            var role = new ApplicationRole { Name = roleName.Role };
            var result = await roleManager.CreateAsync(role);

            return Ok(result);
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(RegisterResponse))]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerRequest)
        {
            var result = await RegisterAsync(registerRequest);
            
            return result.Success ? Ok(result) : BadRequest(result.Message);
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponse))]
        public async Task<IActionResult> Login([FromBody] LoginDto loginRequest)
        {
            var result = await LoginAsync(loginRequest);

            return result.Success ? Ok(result) : BadRequest("Login failed");
        }

        private async Task<RegisterResponse> RegisterAsync(RegisterDto registerRequest)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(registerRequest.Email);

                if (user is not null)
                    return new RegisterResponse { Message = "User already exists", Success = false };

                var businessInfo = new BusinessInfo
                {
                    BusinessName = registerRequest.BusinessName,
                    Category = registerRequest.Category,
                    Description = registerRequest.Description,
                    FoundedYear = null
                };

                var personalInfo = new PersonalInfo
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                };

                await context.BusinessInfos.InsertOneAsync(businessInfo);

                user = new ApplicationUser
                {
                    PersonalInfo = personalInfo,
                    Email = registerRequest.Email,
                    UserName = registerRequest.Email,
                    BusinessInfoId = businessInfo.Id,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                };

                var result = await userManager.CreateAsync(user, registerRequest.Password);

                if (!result.Succeeded)
                {
                    await context.BusinessInfos.DeleteOneAsync(b => b.Id == businessInfo.Id);
                    return new RegisterResponse { Message = "User creation failed", Success = false };
                }

                var update = Builders<BusinessInfo>.Update.Set(b => b.UserId, user.Id);
                await context.BusinessInfos.UpdateOneAsync(
                    b => b.Id == businessInfo.Id,
                    update
                );

                BusinessInfoForMicroservice businessInfoForMicroservice = new()
                {
                    user_id = user.Id.ToString(),
                    name = businessInfo.BusinessName,
                    category = businessInfo.Category,
                    description = businessInfo.Description,
                    location = new LocationChangeForDamian
                    {
                        display_name = businessInfo.Location?.DisplayName ?? string.Empty,
                        street = businessInfo.Location?.Street ?? string.Empty,
                        city = businessInfo.Location?.City ?? string.Empty,
                        state = businessInfo.Location?.State ?? string.Empty,
                        postcode = businessInfo.Location?.Postcode ?? string.Empty,
                        country = businessInfo.Location?.Country ?? string.Empty,
                        latitude = businessInfo.Location?.Latitude ?? 0.0,
                        longitude = businessInfo.Location?.Longitude ?? 0.0
                    },
                    company_size = (int)businessInfo.CompanySize,
                    founded_year = 2008,
                    business_image_urls = businessInfo.BusinessImageUrls,
                    social_media = businessInfo.SocialMedia
                };

                await MicroserviceHandler.SendBusinessInfo(businessInfoForMicroservice);

                var addRoleResult = await userManager.AddToRoleAsync(user, "User");

                if (!addRoleResult.Succeeded)
                    return new RegisterResponse { Message = "Failed to assign role", Success = false };

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.Email!),
                    new(JwtRegisteredClaimNames.Jti, ObjectId.GenerateNewId().ToString()),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var roles = await userManager.GetRolesAsync(user);
                var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
                claims.AddRange(roleClaims);

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddHours(1);

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

                return new RegisterResponse
                {
                    Message = "User registered successfully",
                    Success = true,
                    User = new ApplicationUserDto
                    {
                        FirstName = user.PersonalInfo.FirstName,
                        LastName = user.PersonalInfo.LastName,
                        Email = user.Email!,
                        AvatarUrl = user.PersonalInfo.ProfilePictureUrl ?? string.Empty,
                        UserId = user.Id.ToString(),
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        BusinessInfo = new UserBusinessInfoDto
                        {
                            Name = businessInfo.BusinessName ?? string.Empty,
                            Description = businessInfo.Description ?? string.Empty,
                            Location = businessInfo.Location?.DisplayName ?? string.Empty,
                            Category = businessInfo.Category ?? string.Empty
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return new RegisterResponse { Message = "Registration failed", Success = false };
            }
        }

        private async Task<LoginResponse> LoginAsync(LoginDto loginRequest)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(loginRequest.Email);

                if (user is null || !await userManager.CheckPasswordAsync(user, loginRequest.Password))
                    return new LoginResponse { Success = false };

                BusinessInfo? businessInfo = null;
                if (user.BusinessInfoId != ObjectId.Empty)
                {
                    businessInfo = await context.BusinessInfos.Find(b => b.Id == user.BusinessInfoId).FirstOrDefaultAsync();
                }

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.Email!),
                    new(JwtRegisteredClaimNames.Jti, ObjectId.GenerateNewId().ToString()),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var roles = await userManager.GetRolesAsync(user);
                var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
                claims.AddRange(roleClaims);

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddHours(1);

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

                return new LoginResponse
                {
                    Success = true,
                    User = new ApplicationUserDto
                    {
                        FirstName = user.PersonalInfo.FirstName,
                        LastName = user.PersonalInfo.LastName,
                        Email = user.Email!,
                        AvatarUrl = user.PersonalInfo.ProfilePictureUrl ?? string.Empty,
                        UserId = user.Id.ToString(),
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        BusinessInfo = new UserBusinessInfoDto
                        {
                            Name = businessInfo?.BusinessName ?? string.Empty,
                            Description = businessInfo?.Description ?? string.Empty,
                            Location = businessInfo?.Location?.DisplayName.ToString() ?? string.Empty,
                            Category = businessInfo?.Category ?? string.Empty
                        }
                    }
                };
            } catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new LoginResponse { Success = false };
            }
        }
    }
}
