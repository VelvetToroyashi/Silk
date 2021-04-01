using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Silk.Core.Discord.Commands.Miscellaneous
{
    [Serializable]
    public class ChangelogFile
    {
        public string Description { get; set; }
        public string Changes { get; set; }

        public static ChangelogFile DeserializeChangeLog()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot",
                "ChangeLogs", "JSON");
            FileInfo? fileInfo = new DirectoryInfo(path).GetFiles().OrderBy(f => f.LastWriteTime).LastOrDefault();
            string file = File.ReadAllText(fileInfo.FullName);
            return JsonConvert.DeserializeObject<ChangelogFile>(file);
        }

        public static void ConvertToJSON()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot",
                "ChangeLogs", "Unformatted");
            string pathJSON = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SilkBot", "ChangeLogs", "JSON");
            FileInfo? fileInfo = new DirectoryInfo(path).GetFiles().OrderBy(f => f.LastWriteTime).LastOrDefault();
            string[] fileContent = File.ReadAllLines(fileInfo.FullName);


            string desc = string.Join('\n', fileContent.TakeWhile(f => f.Length > 1));
            string changes = string.Join('\n',
                fileContent.Skip(desc.Split('\n').Length + 1).Take(fileContent.Count() - 1));
            var changeLog = new ChangelogFile
            {
                Description = desc,
                Changes = $"```diff\n{changes}```"
            };

            string fileToWrite = JsonConvert.SerializeObject(changeLog, Formatting.Indented);

            File.WriteAllText(Path.Combine(pathJSON, $"Changelog{Directory.GetFiles(path).Length}"), fileToWrite);
        }
    }
}