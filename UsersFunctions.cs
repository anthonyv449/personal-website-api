using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyIsolatedFuncApp.Data;
using personal_website_api.Users;
using personal_website_api.Auth;
using UsersEntity = MyIsolatedFuncApp.Data.Users;

namespace personal_website_api
{
    public class UsersFunctions
    {
        private readonly ILogger<UsersFunctions> _logger;
        private readonly MyDbContext _db;

        public UsersFunctions(ILogger<UsersFunctions> logger, MyDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Function("GetUsers")]
        public async Task<HttpResponseData> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequestData req)
        {
            var users = await GetUsersLogic.Execute(_db);
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(users);
            return res;
        }

        [Function("CreateUser")]
        public async Task<HttpResponseData> CreateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequestData req)
        {
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            var newUser = await req.ReadFromJsonAsync<UsersEntity>();
            if (newUser == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var created = await CreateUserLogic.Execute(_db, newUser);
            var res = req.CreateResponse(HttpStatusCode.Created);
            await res.WriteAsJsonAsync(created);
            return res;
        }

        [Function("UpdateUser")]
        public async Task<HttpResponseData> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{id:int}")] HttpRequestData req,
            int id)
        {
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            var updated = await req.ReadFromJsonAsync<UsersEntity>();
            if (updated == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var result = await UpdateUserLogic.Execute(_db, id, updated);
            if (result == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(result);
            return res;
        }

        [Function("DeleteUser")]
        public async Task<HttpResponseData> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "users/{id:int}")] HttpRequestData req,
            int id)
        {
            var auth = await AuthorizationHelper.RequireAdmin(req, _db);
            if (auth != null) return auth;

            var success = await DeleteUserLogic.Execute(_db, id);
            return req.CreateResponse(success ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        }
    }
}
