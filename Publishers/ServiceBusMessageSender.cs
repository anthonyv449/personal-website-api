using Azure.Messaging.ServiceBus;

namespace personal_website_api.Publishers;

public class ServiceBusMessageSender : IServiceBusMessageSender
{
    private readonly ServiceBusSender _sender;

    public ServiceBusMessageSender(ServiceBusClient client)
    {
        _sender = client.CreateSender("article-views");
    }

    public Task SendMessageAsync(ServiceBusMessage message) => _sender.SendMessageAsync(message);
}
