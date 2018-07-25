using System.Collections.Generic;

namespace Parser.Entities
{
    public class Section
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public IEnumerable<Choice> Choices { get; set; }
    }
}
