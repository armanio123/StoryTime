using Newtonsoft.Json;
using StoryTime.Parser;
using System.IO;

namespace StoryTime.TestClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var file = File.OpenText("../../../../../stories/sample.md");

            var parser = new MarkdownStoryParser();
            var story = parser.Parse(file.ReadToEnd());

            var json = JsonConvert.SerializeObject(story);
        }
    }
}
