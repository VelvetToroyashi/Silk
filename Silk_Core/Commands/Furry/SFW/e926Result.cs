using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SilkBot.Commands.Furry.SFW
{
    public class e926Result
    {
        [JsonProperty("posts", NullValueHandling = NullValueHandling.Ignore)]
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public File File { get; set; }

        [JsonProperty("preview", NullValueHandling = NullValueHandling.Ignore)]
        public Preview Preview { get; set; }

        [JsonProperty("sample", NullValueHandling = NullValueHandling.Ignore)]
        public Sample Sample { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public Score Score { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public Tags Tags { get; set; }

        [JsonProperty("locked_tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> LockedTags { get; set; }

        [JsonProperty("change_seq", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChangeSeq { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public Flags Flags { get; set; }

        [JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
        public Rating? Rating { get; set; }

        [JsonProperty("fav_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? FavCount { get; set; }

        [JsonProperty("sources", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Sources { get; set; }

        [JsonProperty("pools", NullValueHandling = NullValueHandling.Ignore)]
        public List<long> Pools { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Relationships Relationships { get; set; }

        [JsonProperty("approver_id")]
        public long? ApproverId { get; set; }

        [JsonProperty("uploader_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? UploaderId { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("comment_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? CommentCount { get; set; }

        [JsonProperty("is_favorited", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFavorited { get; set; }

        [JsonProperty("has_notes", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasNotes { get; set; }

        [JsonProperty("duration")]
        public double? Duration { get; set; }
    }

    public class File
    {
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
        public Ext? Ext { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("md5", NullValueHandling = NullValueHandling.Ignore)]
        public string Md5 { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    public class Flags
    {
        [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Pending { get; set; }

        [JsonProperty("flagged", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Flagged { get; set; }

        [JsonProperty("note_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NoteLocked { get; set; }

        [JsonProperty("status_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? StatusLocked { get; set; }

        [JsonProperty("rating_locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RatingLocked { get; set; }

        [JsonProperty("deleted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Deleted { get; set; }
    }

    public class Preview
    {
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    public class Relationships
    {
        [JsonProperty("parent_id")]
        public long? ParentId { get; set; }

        [JsonProperty("has_children", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasChildren { get; set; }

        [JsonProperty("has_active_children", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasActiveChildren { get; set; }

        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<long> Children { get; set; }
    }

    public class Sample
    {
        [JsonProperty("has", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Has { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("alternates", NullValueHandling = NullValueHandling.Ignore)]
        public Alternates Alternates { get; set; }
    }

    public class Alternates
    {
        [JsonProperty("480p", NullValueHandling = NullValueHandling.Ignore)]
        public The480P The480P { get; set; }

        [JsonProperty("original", NullValueHandling = NullValueHandling.Ignore)]
        public Original Original { get; set; }
    }

    public class Original
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("urls", NullValueHandling = NullValueHandling.Ignore)]
        public List<Uri> Urls { get; set; }
    }

    public class The480P
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("urls", NullValueHandling = NullValueHandling.Ignore)]
        public List<Uri> Urls { get; set; }
    }

    public class Score
    {
        [JsonProperty("up", NullValueHandling = NullValueHandling.Ignore)]
        public long? Up { get; set; }

        [JsonProperty("down", NullValueHandling = NullValueHandling.Ignore)]
        public long? Down { get; set; }

        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long? Total { get; set; }
    }

    public class Tags
    {
        [JsonProperty("general", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> General { get; set; }

        [JsonProperty("species", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Species { get; set; }

        [JsonProperty("character", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Character { get; set; }

        [JsonProperty("copyright", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Copyright { get; set; }

        [JsonProperty("artist", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Artist { get; set; }

        [JsonProperty("invalid", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Invalid { get; set; }

        [JsonProperty("lore", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Lore { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Meta { get; set; }
    }

    public enum Ext { Gif, Jpg, Png, Webm }

    public enum Rating { S }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ExtConverter.Singleton,
                RatingConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ExtConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Ext) || t == typeof(Ext?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "gif":
                    return Ext.Gif;
                case "jpg":
                    return Ext.Jpg;
                case "png":
                    return Ext.Png;
                case "webm":
                    return Ext.Webm;
            }
            throw new Exception("Cannot unmarshal type Ext");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Ext)untypedValue;
            switch (value)
            {
                case Ext.Gif:
                    serializer.Serialize(writer, "gif");
                    return;
                case Ext.Jpg:
                    serializer.Serialize(writer, "jpg");
                    return;
                case Ext.Png:
                    serializer.Serialize(writer, "png");
                    return;
                case Ext.Webm:
                    serializer.Serialize(writer, "webm");
                    return;
            }
            throw new Exception("Cannot marshal type Ext");
        }

        public static readonly ExtConverter Singleton = new ExtConverter();
    }

    internal class RatingConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Rating) || t == typeof(Rating?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "s")
            {
                return Rating.S;
            }
            throw new Exception("Cannot unmarshal type Rating");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Rating)untypedValue;
            if (value == Rating.S)
            {
                serializer.Serialize(writer, "s");
                return;
            }
            throw new Exception("Cannot marshal type Rating");
        }

        public static readonly RatingConverter Singleton = new RatingConverter();
    }
}
