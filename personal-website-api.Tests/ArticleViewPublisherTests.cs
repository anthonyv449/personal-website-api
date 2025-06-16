using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging.Abstractions;
using personal_website_api.Publishers;

namespace personal_website_api.Tests;

public class ArticleViewPublisherTests
{
    private class FakeSender : IServiceBusMessageSender
    {
        public ServiceBusMessage? LastMessage;
        public Task SendMessageAsync(ServiceBusMessage message)
        {
            LastMessage = message;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PublishAsync_SendsSerializedMessage()
    {
        var sender = new FakeSender();
        var publisher = new ArticleViewPublisher(sender, NullLogger<ArticleViewPublisher>.Instance);
        var msg = new ArticleViewMessage(42, "u1", DateTime.UtcNow);

        await publisher.PublishAsync(msg);

        Assert.NotNull(sender.LastMessage);
        var body = sender.LastMessage!.Body.ToString();
        var deserialized = JsonSerializer.Deserialize<ArticleViewMessage>(body);
        Assert.Equal(msg.ArticleId, deserialized!.ArticleId);
        Assert.Equal(msg.ViewerId, deserialized.ViewerId);
    }
}
