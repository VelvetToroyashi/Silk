using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SilkBot.Commands.Miscellaneous
{
    [Serializable]
    public class ChangelogFile
    {

        public string Description { get; set; }
        public string Changes { get; set; }

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


            
            var desc = string.Join('\n', fileContent.TakeWhile(f => (f.Length > 1)));
            var changes = string.Join('\n', fileContent.Skip(desc.Split('\n').Length + 1).Take(fileContent.Count() - 1));
            var changeLog = new ChangelogFile 
            {
                Description = desc,
                Changes = $"```diff\n{changes}```"
            };

            var fileToWrite = JsonConvert.SerializeObject(changeLog, Formatting.Indented);

            File.WriteAllText(Path.Combine(pathJSON, $"Changelog{Directory.GetFiles(path).Length}"), fileToWrite);

        }
    }
}
