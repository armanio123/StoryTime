using Parser;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CoreBot
{
    [Serializable]
    public class MockApi
    {
        private Story story1;
        private Story story2;

        public MockApi()
        {
            try
            {
                Stream file1 = null;
                Stream file2 = null;
                using (var client = new WebClient())
                {
                    file1 = client.OpenRead("https://raw.githubusercontent.com/armanio123/StoryTime/master/stories/sample.md");
                    file2 = client.OpenRead("https://raw.githubusercontent.com/armanio123/StoryTime/master/stories/sample2.md");
                }

                var parser = new MarkdownStoryParser();
                story1 = parser.Parse(new StreamReader(file1).ReadToEnd());
                story2 = parser.Parse(new StreamReader(file2).ReadToEnd());
            }
            catch (Exception ex)
            {
                var x = ex;
            }
        }

        public Section GetStartingSection(string storyId)
        {
            return GetStory(storyId).Sections.Values.First();
        }

        public Dictionary<string, dynamic> GetStartingStats(string storyId)
        {
            return GetStory(storyId).Stats;
        }

        public Section GetSectionById(string id, string storyId)
        {
            return GetStory(storyId).Sections.FirstOrDefault(x => x.Key == id).Value;
        }

        public string GetStoryTitleAndAuthor(string storyId)
        {
            return string.Format("{0} {1}. ", GetStory(storyId).Title, GetStory(storyId).Author);
        }

        private Story GetStory(string id)
        {
            return id == "0" ? story1 : story2;
        }
    }
}