namespace GracelineCMS.Domain.Auth
{
    public interface ITokenHandler
    {
        string CreateToken(string email);
        Dictionary<string, string> ValidateToken(string jwtToken);
    }
}
