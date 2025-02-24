namespace GracelineCMS.Infrastructure.Auth
{
    public class AuthCodeValidationResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
