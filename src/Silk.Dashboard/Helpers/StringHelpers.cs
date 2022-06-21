using System.Linq.Expressions;
using Humanizer;

namespace Silk.Dashboard.Helpers;

public static class StringHelpers
{
    private const LetterCasing DefaultLetterCasing = LetterCasing.Title;

    public static string LabelFor(string @string, LetterCasing letterCasing = DefaultLetterCasing)
    {
        return @string.Humanize(DefaultLetterCasing);
    }

    public static string LabelFor<T>(Expression<Func<T>> expression, LetterCasing letterCasing = DefaultLetterCasing)
    {
        if (expression.Body is not MemberExpression me)
            throw new ArgumentException("Expression must be a lambda accessing a member");

        return LabelFor(me.Member.Name);
    }
}