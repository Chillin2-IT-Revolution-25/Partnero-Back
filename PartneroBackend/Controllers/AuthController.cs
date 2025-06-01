using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using PartneroBackend.Data;
using PartneroBackend.Models;
using PartneroBackend.Models.DTOs.AuthDTOs;
using PartneroBackend.Models.InfoClasses;
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
                {
                    return new RegisterResponse { Message = "User already exists", Success = false };
                }

                var businessInfo = new BusinessInfo
                {
                    BusinessName = registerRequest.BusinessName,
                    Category = registerRequest.Category,
                    Description = registerRequest.Description,
                    CompanySize = null,
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

                var addRoleResult = await userManager.AddToRoleAsync(user, "User");

                if (!addRoleResult.Succeeded)
                {
                    return new RegisterResponse { Message = "Failed to assign role", Success = false };
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

                return new RegisterResponse
                {
                    Message = "User registered successfully",
                    Success = true,
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    Email = user.Email,
                    UserId = user.Id.ToString()
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
                {
                    return new LoginResponse { Success = false };
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
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    Email = user.Email!,
                    UserId = user.Id.ToString()
                };
            } catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new LoginResponse { Success = false, AccessToken = string.Empty, Email = string.Empty, UserId = string.Empty };
            }
        }
    }
}
