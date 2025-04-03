using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostgresApi.Functions;

public class UserFunctions
{
    private readonly AppDbContext _context;

    public UserFunctions(AppDbContext context)
    {
        _context = context;
    }

    [Function("GetUsers")]
    public async Task<HttpResponseData> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("GetUsers");
        var users = await _context.Users.ToListAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(users);
        return response;
    }

    [Function("GetUserById")]
    public async Task<HttpResponseData> GetUserById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id:int}")] HttpRequestData req,
        int id)
    {
        var user = await _context.Users.FindAsync(id);
        var response = req.CreateResponse();

        if (user == null)
        {
            response.StatusCode = HttpStatusCode.NotFound;
            return response;
        }

        await response.WriteAsJsonAsync(user);
        return response;
    }

    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequestData req)
    {
        var user = await req.ReadFromJsonAsync<User>();
        if (user == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid user data.");
            return badResponse;
        }

        user.Id = 0;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(user);
        return response;
    }

    [Function("UpdateUser")]
    public async Task<HttpResponseData> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/{id:int}")] HttpRequestData req,
        int id)
    {
        var updatedUser = await req.ReadFromJsonAsync<User>();
        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            return notFound;
        }

        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;

        await _context.SaveChangesAsync();
        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("DeleteUser")]
    public async Task<HttpResponseData> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "user/{id:int}")] HttpRequestData req,
        int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
