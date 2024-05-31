using ValidationService.Infrastructure.OpenAi;

namespace ValidationService.Application;

public partial record ValidationResult
{
    public bool IsValid => !OffensiveElements?.Any() ?? false;

    public string? Explanation { get; init; }
    public string? Offensiveness { get; init; }
    public string? SuggestedAlternative { get; init; }
    public IReadOnlyList<string>? OffensiveElements { get; init; }
}

public partial record ValidationResult
{
    public static ValidationResult EmptyContent()
    {
        return new ValidationResult
        {
            Explanation = "Message body can't be empty",
            Offensiveness = "None",
            OffensiveElements = [],
            SuggestedAlternative = string.Empty
        };
    }

    /// <summary>
    /// Represents the case when validation wasn't completed successfully
    /// </summary>
    /// <param name="msg">Custom message to represent validation provider error</param>
    /// <returns></returns>
    public static ValidationResult NotCompleted(string? msg = null)
    {
        return new ValidationResult
        {
            Explanation = msg ?? "Validation service not available",
            Offensiveness = "Unknown",
            OffensiveElements = [],
            SuggestedAlternative = string.Empty
        };
    }

    public static ValidationResult Offensive(OpenAiResponse ar)
    {
        return new ValidationResult
        {
            Explanation = ar.Explanation,
            Offensiveness = ar.OffensivenessLevel,
            OffensiveElements = ar.OffensiveElements,
            SuggestedAlternative = ar.SuggestedAlternative
        };
    }

    public static implicit operator ValueTask<ValidationResult>(ValidationResult vr)
        => ValueTask.FromResult(vr);

    public static ValidationResult Safe()
    {
        return new ValidationResult();
    }
}
