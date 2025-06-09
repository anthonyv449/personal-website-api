using System.Threading.Tasks;

namespace personal_website_api.Auth
{
    public interface ITokenValidator
    {
        Task<AuthTokenPayload> ValidateAsync(string token);
    }
}
