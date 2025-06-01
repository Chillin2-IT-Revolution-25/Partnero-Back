using PartneroBackend.Models.InfoClasses;

namespace PartneroBackend.Models.DTOs
{
    public class ApplicationUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public UserBusinessInfoDto? BusinessInfo { get; set; }
    }

    public class UserBusinessInfoDto
    {
        public string? Name { get;set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; }
    }
}
