namespace EduGame.DTOs
{
    public class PartnerDto : BaseRegistrationDto
    {
        public string Organization { get; set; } = string.Empty;
        
        public string TypeOfCooperation { get; set; } = string.Empty;
    }
}