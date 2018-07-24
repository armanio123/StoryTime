using System.Collections.Generic;

namespace StoryTime.Parser.Entities
{
    public class Story
    {
        public Dictionary<string, Section> Sections { get; set; }

        public Dictionary<string, dynamic> Stats { get; set; }

        public string Title { get; set; }
    }
}
