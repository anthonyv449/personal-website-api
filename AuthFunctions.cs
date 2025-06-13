using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ITokenValidator _validator;

        public AuthFunctions(ILogger<AuthFunctions> logger, MyDbContext db, ITokenValidator validator)
        {
            _logger = logger;
            _db = db;
            _validator = validator;
        }

        private class TokenRequest { public string? IdToken { get; set; } }

        private static string? GetSessionId(HttpRequestData req)
        {
            if (req.Headers.TryGetValues("Cookie", out var values))
            {
                var cookieHeader = System.Linq.Enumerable.FirstOrDefault(values);
                if (cookieHeader != null)
                {
                    foreach (var part in cookieHeader.Split(';'))
                    {
                        var trimmed = part.Trim();
                        if (trimmed.StartsWith("session="))
                        {
                            return trimmed.Substring("session=".Length);
                        }
                    }
                }
            }
            return null;
        }

        [Function("MicrosoftLogin")]
        public async Task<HttpResponseData> MicrosoftLogin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/microsoft")] HttpRequestData req)
        {
            var body = await req.ReadFromJsonAsync<TokenRequest>();
            if (body?.IdToken == null)
            {
                var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRes.WriteAsJsonAsync(new
                {
                    message = "idToken is required",
                    stackTrace = Environment.StackTrace
                });
                return badRes;
            }

            AuthTokenPayload payload;
            try
            {
                payload = await _validator.ValidateAsync(body.IdToken);
            }
            catch (Exception ex)
            {
                var unauthorizedRes = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedRes.WriteAsJsonAsync(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
                return unauthorizedRes;
            }

            var user = await Auth.LoginWithJwtLogic.Execute(_db, payload.Name ?? "Unknown", payload.Email);

            // create session in database
            var session = await CreateSessionLogic.Execute(_db, user.Id);
            var okRes = req.CreateResponse(HttpStatusCode.OK);
            okRes.Headers.Add("Set-Cookie", $"session={session}; HttpOnly; Secure; SameSite=None; Path=/");
            await okRes.WriteAsJsonAsync(new { user.Id, user.Name, user.Email, user.IsAdmin });
            return okRes;
        }

        [Function("GetMe")]
        public async Task<HttpResponseData> GetMe(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/me")] HttpRequestData req)
        {
            var sessionId = GetSessionId(req);
            if (string.IsNullOrEmpty(sessionId))
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var user = await GetUserBySessionLogic.Execute(_db, sessionId);
            if (user == null)
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(new { user.Id, user.Email, user.Name, user.IsAdmin });
            return res;
        }

        [Function("Logout")]
        public async Task<HttpResponseData> Logout(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/logout")] HttpRequestData req)
        {
            var sessionId = GetSessionId(req);
            if (!string.IsNullOrEmpty(sessionId))
            {
                await DeleteSessionLogic.Execute(_db, sessionId);
            }
            var res = req.CreateResponse(HttpStatusCode.NoContent);
            res.Headers.Add("Set-Cookie", "session=; HttpOnly; Secure; SameSite=None; Path=/; Max-Age=0");
            return res;
        }
    }
}
