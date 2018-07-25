using System.Collections.Generic;

namespace Parser.Entities
{
    public class Choice
    {
        public IEnumerable<StatEffect> Conditions { get; set; }

        public IEnumerable<StatEffect> Effects { get; set; }

        public string Text { get; set; }

        public string SectionKey { get; set; }
    }
}
