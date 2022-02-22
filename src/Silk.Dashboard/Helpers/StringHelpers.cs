using Humanizer;

namespace Silk.Dashboard.Helpers;

public static class StringHelpers
{
    public static string LabelFor(string @string) 
        => @string.Humanize(LetterCasing.Title);
}