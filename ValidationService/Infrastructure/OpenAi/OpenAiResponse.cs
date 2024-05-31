using System.Text.Json.Serialization;

namespace ValidationService.Infrastructure.OpenAi;

public record OpenAiResponse
{
    [JsonPropertyName("offensive_elements")]
    public List<string>? OffensiveElements { get; init; }

    [JsonPropertyName("level_of_offensiveness")]
    public string? OffensivenessLevel { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("suggested_alternative")]
    public string? SuggestedAlternative { get; set; }
}
