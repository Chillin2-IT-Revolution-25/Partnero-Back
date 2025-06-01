namespace PartneroBackend.Models.DTOs.AuthDTOs
{
    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }

        public ApplicationUserDto User { get; set; } = new ApplicationUserDto();
    }
}
