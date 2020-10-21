using System.Collections.Generic;
using System.Linq;

namespace SilkBot.Utilities
{
    public static class SplitExtension
    {
        public static IEnumerable<string> BlockSplit(this string s, string seperator, int blockSize)
        {
            var split = s.Split(seperator);
            for(int i = 0; i < split.Length / blockSize; i++)
            {
                yield return string.Join(string.Empty, split.Skip(i * blockSize).Take(blockSize));
            }
        }
    }
}
