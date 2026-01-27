namespace EduGame.Entities
{
    public class Student
    {
        public int StudentId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Education UserEducation { get; set; }
        public string? Motivation { get; set; }
    }
    public enum Education
    {
        Pupil,
        MiddleSchool,
        HighSchool,
        Student,
        Other
    }
}