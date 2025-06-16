namespace personal_website_api.Publishers;

using Azure.Messaging.ServiceBus;

public interface IServiceBusMessageSender
{
    Task SendMessageAsync(ServiceBusMessage message);
}
