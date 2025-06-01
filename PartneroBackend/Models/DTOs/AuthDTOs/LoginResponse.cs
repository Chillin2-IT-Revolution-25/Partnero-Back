namespace PartneroBackend.Models.DTOs.AuthDTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public ApplicationUserDto User { get; set; } = new ApplicationUserDto();
    }
}
