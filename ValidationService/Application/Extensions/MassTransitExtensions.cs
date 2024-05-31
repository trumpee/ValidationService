using MassTransit;

namespace ValidationService.Application.Extensions;

public static class MassTransitExtensions
{
    public static async Task<ISendEndpoint> GetQueue(
        this ISendEndpointProvider provider, string queueName)
    {
        var uri = new Uri($"queue:{queueName}");
        return await provider.GetSendEndpoint(uri);
    }
}
