using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace personal_website_api.Publishers;

public class ArticleViewPublisher
{
    private readonly IServiceBusMessageSender _sender;
    private readonly ILogger<ArticleViewPublisher> _logger;

    public ArticleViewPublisher(IServiceBusMessageSender sender, ILogger<ArticleViewPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [Function("PublishArticleView")]
    public async Task<HttpResponseData> PublishArticleView(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "articles/viewed")] HttpRequestData req)
    {
        var message = await req.ReadFromJsonAsync<ArticleViewMessage>();
        await PublishAsync(message);
        return req.CreateResponse(HttpStatusCode.Accepted);
    }

    public async Task PublishAsync(ArticleViewMessage message)
    {
        var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
        await _sender.SendMessageAsync(serviceBusMessage);
        _logger.LogInformation("Published article view for {ArticleId}", message.ArticleId);
    }
}
