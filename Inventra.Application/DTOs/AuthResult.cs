namespace Inventra.Application.DTOs
{
    public class AuthResult
    {
        public bool Succeeded { get; init; }
        public bool IsLockedOut { get; init; }
        public bool RequiresTwoFactor { get; init; }
        public IEnumerable<string> Errors { get; init; } = [];

        public static AuthResult Success() => new() { Succeeded = true };
        public static AuthResult Failure(IEnumerable<string> errors)
            => new() { Succeeded = false, Errors = errors };
        public static AuthResult LockedOut()
            => new() { Succeeded = false, IsLockedOut = true };
    }
}
    