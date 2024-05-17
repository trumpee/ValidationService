using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trumpee.MassTransit.Messages.Notifications;

namespace ValidationService;

public class OffenciveContentValidationService(
    OffensiveContentValidator validator,
    ILogger<OffenciveContentValidationService> logger)
{
    public bool IsOffencive(Notification message)
    {
        var vars = message.Content!.Variables?.Select(x => x.Value.Value).ToList();

        var isOffensive = true;
        if (vars is { Count: > 0 })
        {
            foreach (var var in vars)
            {
                isOffensive = validator.ContainsOffensiveContent(
                    JsonSerializer.Serialize(var));

                if (isOffensive)
                {
                    logger.LogDebug("The text contains offensive content. | {Variable}", var);
                }
                else
                {
                    logger.LogDebug("The text is clean. | {Variable}", var);
                }
            }
        }

        return isOffensive;
    }
}
