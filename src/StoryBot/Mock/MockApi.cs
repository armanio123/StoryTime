using Parser;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace StoryBot.Mock
{
    [Serializable]
    public class MockApi
    {
        private Story story;

        public MockApi()
        {
            try
            {
                Stream file = null;
                using (var client = new WebClient())
                {
                    file = client.OpenRead("https://raw.githubusercontent.com/armanio123/StoryTime/master/stories/sample.md");
                }

                var parser = new MarkdownStoryParser();
                story = parser.Parse(new StreamReader(file).ReadToEnd());
            }catch(Exception ex)
            {
                var x = ex;
            }
        }

        public Section GetStartingSection()
        {
            return story.Sections.Values.First();
        }

        public Dictionary<string, dynamic> GetStartingStats()
        {
            return story.Stats;
        }

        public Section GetSectionById(string id)
        {
            return story.Sections.FirstOrDefault(x => x.Key == id).Value;
        }

        public string GetStoryTitleAndAuthor()
        {
            return string.Format("{0} {1}. ", story.Title, story.Author);
        }
    }
}