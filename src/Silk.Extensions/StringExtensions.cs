#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Silk.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> BlockSplit(this string s, string seperator, int blockSize)
        {
            string[] split = s.Split(seperator);
            for (var i = 0; i < split.Length / blockSize; i++)
                yield return string.Join(string.Empty, split.Skip(i * blockSize).Take(blockSize));
        }

        public static IEnumerable<string> Split(this string s, char delimeter)
        {
            for (var i = 0; i < s.Length; i++)
                if (s[i] == delimeter)
                    yield return s[i..^s.Length].TakeWhile(c => c != delimeter).Join();
        }
    }
}