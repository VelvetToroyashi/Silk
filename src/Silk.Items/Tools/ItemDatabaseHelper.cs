#region

using Newtonsoft.Json;
using Silk.Items.Entities;

#endregion

namespace Silk.Items.Tools
{
    /// <summary>
    /// Wrapper utility class for serializing and deserializing items, which double as models.
    /// </summary>
    public static class ItemDatabaseHelper
    {


        public static string Serialize(Entity entity) =>
            JsonConvert.SerializeObject(entity, Formatting.Indented);

        public static T Deserialize<T>(string obj) where T : Entity => JsonConvert.DeserializeObject<T>(obj);

    }
}