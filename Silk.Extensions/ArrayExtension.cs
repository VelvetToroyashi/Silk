#region

using System.Linq;

#endregion

namespace Silk.Extensions
{
    public static class ArrayExtension
    {
        private static int index;

        public static T GetNext<T>(this T[] o)
        {
            index++;
            return index >= o.Length ? o.Last() : o[index];
        }
    }
}