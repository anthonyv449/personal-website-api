using Google.Apis.Auth;
using System.Threading.Tasks;

namespace personal_website_api.Auth
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleJsonWebSignature.Payload> ValidateAsync(string token);
    }
}
