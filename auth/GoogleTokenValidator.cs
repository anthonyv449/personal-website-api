using Google.Apis.Auth;
using System.Threading.Tasks;

namespace personal_website_api.Auth
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        public Task<GoogleJsonWebSignature.Payload> ValidateAsync(string token)
        {
            return GoogleJsonWebSignature.ValidateAsync(token);
        }
    }
}
