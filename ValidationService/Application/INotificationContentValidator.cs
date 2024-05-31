using Trumpee.MassTransit.Messages.Notifications;

namespace ValidationService.Application;

public interface INotificationContentValidator
{
    ValueTask<ValidationResult> ValidateNotificationContent(Notification notification);
}
