using MassTransit;
using Microsoft.Extensions.Logging;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Messages.Analytics;
using Trumpee.MassTransit.Messages.Analytics.Validation;
using Trumpee.MassTransit.Messages.Notifications;
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
        var validationInfo = new ValidationInfoDto()
        {
            Explanation = validationResult.Explanation,
            Offensiveness = validationResult.Offensiveness,
            OffensiveElements = validationResult.OffensiveElements,
            SuggestedAlternative = validationResult.SuggestedAlternative
        };

        var analyticsEvent = Validation.Failed(
            "Trumpee Validation Service", notification.NotificationId, validationInfo);

        await Task.WhenAll(
            SendToOffensiveQueue(notification),
            SendToAnalyticsQueue(analyticsEvent)
        );
    }

    private async Task HandleSafeMessage(Notification notification)
    {
        var analyticsEvent = Validation.Passed(
            "Trumpee Validation Service", notification.NotificationId);

        await Task.WhenAll(
            SendToPrioritizationQueue(notification),
            SendToAnalyticsQueue(analyticsEvent)
        );
    }

    private async Task SendToPrioritizationQueue(Notification notification)
    {
        var queue = await sendEndpoint.GetQueue(QueueNames.Services.PrioritizationQueueName);
        await queue.Send(notification);
    }

    private async Task SendToOffensiveQueue(Notification notification)
    {
        var queue = await sendEndpoint.GetQueue("offensive");
        await queue.Send(notification);
    }

    private async Task SendToAnalyticsQueue<TPayload>(
        AnalyticsEvent<TPayload> analyticsEvent)
    {
        var queue = await sendEndpoint.GetQueue(
            QueueNames.Analytics.Notifications(typeof(TPayload)));
        await queue.Send(analyticsEvent);
    }
}
