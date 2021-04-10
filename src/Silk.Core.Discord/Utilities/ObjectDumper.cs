using Newtonsoft.Json;

namespace Silk.Core.Discord.Utilities
{
    public class ObjectDumper
    {
        public static string DumpAsJson(object o, bool indented = true)
        {
            return JsonConvert.SerializeObject(o,
                indented ? Formatting.Indented : Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    DateFormatString = "h:mm:ss ff tt"
                });
        }
    }
}