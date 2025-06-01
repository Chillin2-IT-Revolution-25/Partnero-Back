namespace PartneroBackend.Models.InfoClasses
{
    public class PersonalInfo
    {
        public string? ProfilePictureUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string? Website { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
    }
}
