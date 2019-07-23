using Parser;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BotService.Shared
{
    [Serializable]
    public class MockApi
    {
        private Story story1;
        private Story story2;
        private Story story3;

        public MockApi()
        {
            //try
            //{
                Stream file1 = null;
                Stream file2 = null;
                string file3 = null;

                using (var client = new WebClient())
                {
                    file1 = client.OpenRead("https://raw.githubusercontent.com/armanio123/StoryTime/master/stories/sample.md");
                    file2 = client.OpenRead("https://raw.githubusercontent.com/armanio123/StoryTime/master/stories/sample2.md");
                }

                using (var fs = File.OpenRead("../../stories/sample3.md"))
                {
                    byte[] b = new byte[1024];
                    var temp = new UTF8Encoding(true);

                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        file3 = temp.GetString(b);
                    }
                }

                var parser = new MarkdownStoryParser();
                story1 = parser.Parse(new StreamReader(file1).ReadToEnd());
                story2 = parser.Parse(new StreamReader(file2).ReadToEnd());
                story3 = parser.Parse(file3);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        public Section GetStartingSection(string storyId)
        {
            return GetStory(storyId).Sections.Values.First();
        }

        public Dictionary<string, dynamic> GetStartingStats(string storyId)
        {
            return GetStory(storyId).Stats;
        }

        public Section GetSectionById(string storyId, string id)
        {
            return GetStory(storyId).Sections.FirstOrDefault(x => x.Key == id).Value;
        }

        public string GetStoryTitleAndAuthor(string storyId)
        {
            return string.Format("{0} {1}. ", GetStory(storyId).Title, GetStory(storyId).Author);
        }

        private Story GetStory(string id)
        {
            switch (id)
            {
                default:
                case "1":
                    return story1;
                case "2":
                    return story2;
                case "3":
                    return story3;
            }
        }
    }
}