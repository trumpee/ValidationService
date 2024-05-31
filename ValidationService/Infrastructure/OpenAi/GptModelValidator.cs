using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using Trumpee.MassTransit.Messages.Notifications;
using ValidationService.Application;
using ValidationService.Infrastructure.OpenAi.Extension;

namespace ValidationService.Infrastructure.OpenAi;

public class GptModelValidator(
    IOpenAIService openAiService,
    ILogger<GptModelValidator> logger) : INotificationContentValidator
{
    private const string Prompt =
        """
        You are an advanced language model specialized in identifying and categorizing offensive language. Your task is to analyze the given text and determine whether it contains offensive or toxic language. Follow these steps to complete the task:
        
        Analyze the Text: Examine the provided text carefully.
        Detect Offensive Language: Identify any words, phrases, or sentences that are offensive, inappropriate, or toxic.
        Categorize the Offensiveness: Classify the level of offensiveness as:
        Mild: Slightly inappropriate or offensive.
        Moderate: Clearly offensive but not extremely harmful.
        Severe: Highly offensive, abusive, or harmful.
        Provide Explanations: For each detected offensive element, explain why it is considered offensive and its potential impact.
        Suggest Alternatives: Where possible, suggest non-offensive alternatives to the detected offensive elements.
        Examples:
        
        Example 1:
        
        Input: "You are an idiot."
        Output:
        Offensive Elements: "idiot"
        Level of Offensiveness: Moderate
        Explanation: The word "idiot" is used as an insult to demean someone's intelligence.
        Suggested Alternative: "You made a mistake."
        
        Example 2:
        
        Input: "Go to hell, you stupid jerk!"
        Output:
        Offensive Elements: "Go to hell", "stupid jerk"
        Level of Offensiveness: Severe
        Explanation: "Go to hell" is a curse phrase wishing someone ill. "Stupid jerk" combines an insult about intelligence with a derogatory term.
        Suggested Alternative: "I disagree with you."
        
        Example 3:
        
        Input: "That idea is terrible."
        Output:
        Offensive Elements: None
        Level of Offensiveness: None
        Explanation: The statement criticizes the idea, not the person, and is not inherently offensive.
        Suggested Alternative: N/A
        Your Task:
        
        Analyze the following text and provide the output in the specified format:
        
        Text to Analyze:
        {TEXT}
        
        Response Format (snake case JSON):
        Offensive Elements: {List offensive words/phrases}
        Level of Offensiveness: {Mild/Moderate/Severe/None}
        Explanation: {Explanation of why it's offensive}
        Suggested Alternative: {Non-offensive alternatives}
        """;

    private readonly IOpenAIService _openAiService = openAiService;
    private readonly ILogger<GptModelValidator> _logger = logger;

    public async ValueTask<ValidationResult> ValidateNotificationContent(Notification notification)
    {
        var isEmptyContent = notification.Content is null ||
                             string.IsNullOrEmpty(notification.Content.Body);

        if (isEmptyContent)
        {
            return ValidationResult.EmptyContent();
        }

        var messageContent =
            $"""
             {notification.Content!.Subject}

             {notification.Content!.Body}
             """;

        var aiResponse = await GetGpt35Conclusion(messageContent);
        return MapToValidationResult(aiResponse);
    }

    private ValidationResult MapToValidationResult(OpenAiResponse? aiResponse)
    {
        if (aiResponse is null)
        {
            return ValidationResult.NotCompleted();
        }

        if (aiResponse.OffensiveElements!.Count != 0)
        {
            return ValidationResult.Offensive(aiResponse);
        }

        return ValidationResult.Safe();
    }

    public async Task<OpenAiResponse?> GetGpt35Conclusion(string message)
    {
        var result = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromUser(Prompt.Replace("{TEXT}", message))
            },
            Model = Models.Gpt_3_5_Turbo
        });

        if (!result.Successful)
        {
            _logger.LogError("Validation provider is not accessible");
            var error = result.Error?.ToMarkdown();
            return new OpenAiResponse
            {
                Explanation = error
            };
        }

        var response = result.Choices.First().Message.Content ?? "";
        if (string.IsNullOrEmpty(response))
        {
            return null;
        }

        Console.WriteLine(response);
        return JsonSerializer.Deserialize<OpenAiResponse>(response);
    }
}
