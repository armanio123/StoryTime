using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoryBot.Models
{
    namespace StoryTime.Shared
    {
        [Serializable]
        public class Story
        {
            public Dictionary<string, Section> Sections { get; set; }

            public Dictionary<string, dynamic> Stats { get; set; }

            public string Title { get; set; }
        }

        [Serializable]
        public class Section
        {
            public string Key { get; set; }

            public string Text { get; set; }

            public IEnumerable<Action> Actions { get; set; }
        }

        [Serializable]
        public class Action
        {
            public IEnumerable<StatEffect> Conditions { get; set; }

            public IEnumerable<StatEffect> Effect { get; set; }

            public string Text { get; set; }

            public string SectionKey { get; set; }
        }

        [Serializable]
        public class StatEffect
        {
            public string Key { get; set; }

            public EffectType EffectType { get; set; }

            public dynamic Value { get; set; }
        }

        public enum EffectType
        {
            None = 0,
            AddOrHave = 1,
            RemoveOrDontHave = 2
        }
    }
}