using Parser.Entities;
using System.Collections.Generic;
using System.Linq;

namespace StoryBot.Mock
{
    public class MockApi
    {
        private Story story;

        public MockApi()
        {
            // TODO replace with parser
            story = new Story();
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