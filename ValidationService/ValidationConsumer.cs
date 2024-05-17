using MassTransit;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Messages.Notifications;

namespace ValidationService;

public class ValidationConsumer(OffenciveContentValidationService validator)
    : IConsumer<Notification>
{
    public async Task Consume(ConsumeContext<Notification> context)
    {
        var isOffensive = validator.IsOffencive(context.Message);
        if (!isOffensive)
        {
            await context.Send(new Uri(QueueNames.PrioritizationQueueName), context.Message);
        }
        else
        {
            await context.Send(new Uri("queue:offensive"), context.Message);
        }
    }
}
