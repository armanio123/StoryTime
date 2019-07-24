using System;
using System.Collections.Generic;

namespace Parser.Entities
{
    [Serializable]
    public class Choice
    {
        // The actions needs to meet the criteria of this conditions.
        // Multiple conditions work as an AND.
        public IEnumerable<StatEffect> Conditions { get; set; }
        
        // If the action is selected, all the effects will apply.
        public IEnumerable<StatEffect> Effects { get; set; }

        public string SectionKey { get; set; }

        public string Text { get; set; }

        // This actions will trigger it's effect and section automatically if met.
        // Multiple triggers work as an OR.
        public IEnumerable<StatEffect> Triggers { get; set; }
    }
}
