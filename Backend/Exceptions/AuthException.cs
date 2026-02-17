namespace EduGame.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string? message) : base(message) {}

        public override string? StackTrace => null;
    }
}