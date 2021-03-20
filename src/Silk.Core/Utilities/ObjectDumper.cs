using Newtonsoft.Json;

namespace Silk.Core.Utilities
{
    public class ObjectDumper
    {
        public static string DumpAsJson(object o) => JsonConvert.SerializeObject(o, Formatting.Indented);
    }
}