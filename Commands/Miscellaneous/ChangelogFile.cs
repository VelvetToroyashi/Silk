using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SilkBot.Commands.Miscellaneous
{
    [Serializable]
    public class ChangelogFile
    {
        public string Version { get; set; }
        public string ShortDescription { get; set; }
        public string Additions { get; set; }
        public string Removals { get; set; }


        public static ChangelogFile DeserializeChangeLog()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ChangeLogs", "JSON");
            var fileInfo = new DirectoryInfo(path).GetFiles().OrderBy(f => f.LastWriteTime).LastOrDefault();
            var file = File.ReadAllText(fileInfo.FullName);
            return JsonConvert.DeserializeObject<ChangelogFile>(file);
        }

        public static void ConvertToJSON()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ChangeLogs", "Unformatted");
            var pathJSON = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ChangeLogs", "JSON");
            var fileInfo = new DirectoryInfo(path).GetFiles().OrderBy(f => f.LastWriteTime).LastOrDefault();
            var fileContent = File.ReadAllLines(fileInfo.FullName);

            var changeLog = new ChangelogFile { Additions = fileContent[1], Removals = fileContent[2], ShortDescription = fileContent[3], Version = fileContent[0] };

            var fileToWrite = JsonConvert.SerializeObject(changeLog, Formatting.Indented);

            File.WriteAllText(Path.Combine(pathJSON, $"Changelog{Directory.GetFiles(path).Length}"), fileToWrite);

        }
    }
}
