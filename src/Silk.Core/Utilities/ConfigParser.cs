using System;
using System.Linq;
using System.Reflection;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.Utilities
{
    internal sealed class ConfigParser //: TextParser
    {
        //public ConfigParser(string currentString) : base(currentString) { }
        private static readonly IniParserConfiguration _config = new() {CaseInsensitive = true, CommentString = "//"};
        private readonly ConfigService _configService;

        public ConfigParser(ConfigService configService) => _configService = configService;

        public void GetConfigInfo(string file)
        {
            var parser = new IniDataParser(_config);
            IniData parseResult = parser.Parse(file);
            GuildConfig model = new();
        }

        public T ParseResult<T>(string input) where T : struct
        {
            MethodInfo[] methods = typeof(T).GetMethods();
            MethodInfo? tryParseMethod = methods.FirstOrDefault(m => m.Name is "TryParse");
            if (tryParseMethod is null) throw new ArgumentException("Not a supported type!");

            object[] @params = {input, null!};

            bool? success = tryParseMethod.Invoke(null, @params) as bool?;
            if (!success!.Value) throw new InvalidOperationException($"Cannot convert from {typeof(string)} to {typeof(T)}");

            return (T) @params[1];
        }


    }

}