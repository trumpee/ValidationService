using System.Text.RegularExpressions;

namespace ValidationService;

public class OffensiveContentValidator
{
    private readonly string[] _offensiveWords = { "fuck", "bitch", "shit" };

    public bool ContainsOffensiveContent(string text)
    {
        text = text.ToLower();

        return _offensiveWords
            .Select(word => $@"\b{Regex.Escape(word)}\b")
            .Any(pattern => Regex.IsMatch(text, pattern));
    }
}