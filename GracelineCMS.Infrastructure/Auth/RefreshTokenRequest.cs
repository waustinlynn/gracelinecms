namespace GracelineCMS.Infrastructure.Auth
{
    public class RefreshTokenRequest
    {
        public required string EmailAddress { get; set; }
        public required string RefreshToken { get; set; }
    }
}
