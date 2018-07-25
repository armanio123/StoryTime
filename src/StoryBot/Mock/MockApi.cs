using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoryBot.Mock
{
    public class MockApi
    {
        Story story;

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