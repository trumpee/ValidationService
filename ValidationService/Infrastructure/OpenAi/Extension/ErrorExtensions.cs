using System.Text;
using OpenAI.ObjectModels.ResponseModels;

namespace ValidationService.Infrastructure.OpenAi.Extension;

public static class ErrorExtensions
{
    public static string ToMarkdown(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var sb = new StringBuilder();

        sb.AppendLine("## Error");
        sb.AppendLine($"- **Code**: `{error.Code}`");
        sb.AppendLine($"- **Param**: `{error.Param}`");
        sb.AppendLine($"- **Type**: `{error.Type}`");
        sb.AppendLine($"- **Line**: `{error.Line}`");

        if (error.Message is not null)
        {
            sb.AppendLine($"- **Message**: `{error.Message}`");
        }

        if (error.Messages.Count != 0)
        {
            sb.AppendLine("- **Messages**:");
            foreach (var message in error.Messages)
            {
                sb.AppendLine($"  - {message}");
            }
        }

        return sb.ToString();
    }
}
