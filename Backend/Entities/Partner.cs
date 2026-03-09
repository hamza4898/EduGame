namespace EduGame.Entities
{
    public class Partner : BaseUser
    {
        public string Organization { get; set; } = string.Empty;

        public string TypeOfCooperation { get; set; } = string.Empty;
    }
}