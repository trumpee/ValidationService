using Trumpee.MassTransit.Messages.Notifications;

namespace ValidationService.Application.Services;

public interface INotificationContentValidationService
{
    Task ProcessNotification(Notification notification);
}
