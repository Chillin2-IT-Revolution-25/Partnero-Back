﻿namespace PartneroBackend.Models.DTOs.AuthDTOs
{
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public string BusinessName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}
