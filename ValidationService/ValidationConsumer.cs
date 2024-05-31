using MassTransit;
using Trumpee.MassTransit.Messages.Notifications;
using ValidationService.Application.Services;

namespace ValidationService;

public class ValidationConsumer(INotificationContentValidationService validator)
    : IConsumer<Notification>
{
    public async Task Consume(ConsumeContext<Notification> context)
    {
        await validator.ProcessNotification(context.Message);
    }
}
