using System.Linq;

namespace SilkBot.Extensions
{
    public static class ArrayExtension
    {
        private static int index = 0;

        public static T GetNext<T>(this T[] o)
        {
            index++;
            return index >= o.Length ? o.Last() : o[index];
        }
    }
}