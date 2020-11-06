using Silk__Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SilkBot.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> BlockSplit(this string s, string seperator, int blockSize)
        {
            var split = s.Split(seperator);
            for(int i = 0; i < split.Length / blockSize; i++)
            {
                yield return string.Join(string.Empty, split.Skip(i * blockSize).Take(blockSize));
            }
        }

        public static IEnumerable<string> Split(this string s, char delimeter)
        {
            for(int i = 0; i < s.Length; i++)
                if (s[i] == delimeter) 
                    yield return s[i..^s.Length].TakeWhile(c => c != delimeter).JoinString();
        }
    }
}
