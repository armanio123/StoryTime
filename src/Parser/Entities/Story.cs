using System;
using System.Collections.Generic;

namespace Parser.Entities
{
    [Serializable]
    public class Story
    {
        public Dictionary<string, Section> Sections { get; set; }

        public Dictionary<string, dynamic> Stats { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }
    }
}
