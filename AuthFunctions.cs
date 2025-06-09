using System;
using System.Net;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;
using personal_website_api.Auth;

namespace personal_website_api
{
    public class AuthFunctions
    {
        private readonly ILogger<AuthFunctions> _logger;
        private readonly MyDbContext _db;
        private readonly IGoogleTokenValidator _validator;

        public AuthFunctions(ILogger<AuthFunctions> logger, MyDbContext db, IGoogleTokenValidator validator)
        {
            _logger = logger;
            _db = db;
            _validator = validator;
        }

        private class TokenRequest { public string? IdToken { get; set; } }

        [Function("GoogleLogin")]
        public async Task<HttpResponseData> GoogleLogin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/google")] HttpRequestData req)
        {
            var body = await req.ReadFromJsonAsync<TokenRequest>();
            if (body?.IdToken == null)
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new
                {
                    message = "idToken is required",
                    stackTrace = Environment.StackTrace
                });
                return res;
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await _validator.ValidateAsync(body.IdToken);
            }
            catch (Exception ex)
            {
                var res = req.CreateResponse(HttpStatusCode.Unauthorized);
                await res.WriteAsJsonAsync(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
                return res;
            }

            var user = await Auth.LoginWithGoogleLogic.Execute(_db, payload.Name ?? "Unknown", payload.Email);

            // create session token - here simple GUID
            var session = System.Guid.NewGuid().ToString();
            var res = req.CreateResponse(HttpStatusCode.OK);
            res.Headers.Add("Set-Cookie", $"session={session}; HttpOnly; Secure; SameSite=None; Path=/");
            await res.WriteAsJsonAsync(new { user.Id, user.Name, user.Email });
            return res;
        }
    }
}
