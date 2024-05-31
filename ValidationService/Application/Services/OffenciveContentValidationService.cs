using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Messages.Notifications;
using Trumpee.MassTransit.Messages.Notifications.Validation;
using ValidationService.Application.Extensions;

namespace ValidationService.Application.Services;

public class NotificationContentValidationService(
    ISendEndpointProvider sendEndpoint,
    INotificationContentValidator validator,
    ILogger<NotificationContentValidationService> logger) : INotificationContentValidationService
{
    public async Task ProcessNotification(Notification notification)
    {
        try
        {
            await ProcessNotificationInternal(notification);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured while processing the notification: {n}", notification.NotificationId);
        }
    }

    private async Task ProcessNotificationInternal(Notification notification)
    {
        var validationResult = await validator.ValidateNotificationContent(notification);
        var isOffensive = validationResult.IsValid;

        var completeProcessingTask = isOffensive
            ? HandleOffenciveMessage(notification, validationResult)
            : HandleSafeMessage(notification);

        await completeProcessingTask;
    }

    private async Task HandleOffenciveMessage(Notification notification, ValidationResult validationResult)
    {
        var analyticsEvent = Validation.Failed(nameof(ValidationConsumer),
            notification.NotificationId, JsonSerializer.Serialize(validationResult));

        await Task.WhenAll(
            SendToOffensiveQueue(notification),
            SendToAnalyticsQueue(analyticsEvent)
        );
    }

    private async Task HandleSafeMessage(Notification notification)
    {
        var analyticsEvent = Validation.Passed(nameof(NotificationContentValidationService), notification.NotificationId);

        await Task.WhenAll(
            SendToPrioritizationQueue(notification),
            SendToAnalyticsQueue(analyticsEvent)
        );
    }

    private async Task SendToPrioritizationQueue(Notification notification)
    {
        var queue = await sendEndpoint.GetQueue(QueueNames.PrioritizationQueueName);
        await queue.Send(notification);
    }

    private async Task SendToOffensiveQueue(Notification notification)
    {
        var queue = await sendEndpoint.GetQueue("offensive");
        await queue.Send(notification);
    }

    private async Task SendToAnalyticsQueue<TPayload>(
        Trumpee.MassTransit.Messages.Event<TPayload> analyticsEvent)
    {
        var queue = await sendEndpoint.GetQueue("analytics");
        await queue.Send(analyticsEvent);
    }
}
