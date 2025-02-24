namespace GracelineCMS.Domain.Entities
{
    public class AuthCode
    {
        public string Id { get; set; }
        public required string Code { get; set; }
        public required string EmailAddress { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public AuthCode()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
