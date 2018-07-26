using Newtonsoft.Json;
using Parser;
using System.IO;

namespace StoryTime.TestClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var file1 = File.OpenText("../../../../../stories/sample.md");
            var file2 = File.OpenText("../../../../../stories/sample2.md");

            var parser = new MarkdownStoryParser();
            var story1 = parser.Parse(file1.ReadToEnd());
            var story2 = parser.Parse(file2.ReadToEnd());

            var json1 = JsonConvert.SerializeObject(story1);
            var json2 = JsonConvert.SerializeObject(story2);
        }
    }
}
