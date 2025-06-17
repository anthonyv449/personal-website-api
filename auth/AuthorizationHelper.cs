using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using MyIsolatedFuncApp.Data;
using personal_website_api.Auth;
using UsersEntity = MyIsolatedFuncApp.Data.Users;

namespace personal_website_api.Auth
{
    public static class AuthorizationHelper
    {
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

        public static async Task<HttpResponseData?> RequireAdmin(HttpRequestData req, MyDbContext db)
        {
            var sessionId = GetSessionId(req);
            UsersEntity? user = null;

            if (!string.IsNullOrEmpty(sessionId))
            {
                user = await GetUserBySessionLogic.Execute(db, sessionId);
                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            else
            {
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                if (!int.TryParse(query["userId"], out var userId))
                {
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }

                user = await db.Users.FindAsync(userId);
                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            if (!user.IsAdmin)
            {
                return req.CreateResponse(HttpStatusCode.Forbidden);
            }

            return null;
        }

        public static async Task<UsersEntity?> GetUserFromSession(HttpRequestData req, MyDbContext db)
        {
            var sessionId = GetSessionId(req);
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            if (!string.IsNullOrEmpty(sessionId))
            {
                return await GetUserBySessionLogic.Execute(db, sessionId);

            }
            else
            {
                if (!int.TryParse(query["userId"], out var userId))
                {
                    return null;
                }
                var user = await db.Users.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }else 
                {
                    return user;
                }
            }            
        }

    }
}
