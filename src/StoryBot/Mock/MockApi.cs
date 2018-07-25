using Parser;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StoryBot.Mock
{
    public class MockApi
    {
        private Story story;

        public MockApi()
        {
            var file = File.OpenText("../../../../../stories/sample.md");

            var parser = new MarkdownStoryParser();
            story = parser.Parse(file.ReadToEnd());
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
    }
}