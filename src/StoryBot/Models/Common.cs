using System.Collections.Generic;

namespace StoryBot.Models
{
    namespace StoryTime.Shared
    {
        public static class ChoiceKeyEquivalents
        {
            private static Dictionary<string, int> dictionary = new Dictionary<string, int>()
            {
                { "1", 0},{ "2", 1},{ "3", 2},{ "4", 3},{ "5", 4},
                { "one", 0},{ "two", 1},{ "three", 2},{ "four", 3},{ "five", 4},
                { "option one", 0},{ "option two", 1},{ "option three", 2},{ "option four", 3},{ "option five", 4},
                { "option 1", 0},{ "option 2", 1},{ "option 3", 2},{ "option 4", 3},{ "option 5", 4},
                { "a", 0},{ "b", 1},{ "c", 2},{ "d", 3},{ "e", 4},
                { "option a", 0},{ "option b", 1},{ "option c", 2},{ "option d", 3},{ "option e", 4},
                { "first", 0},{ "second", 1},{ "third", 2},{ "fourth", 3},{ "fifth", 4},
                { "1st", 0},{ "2nd", 1},{ "3rd", 2},{ "4th", 3},{ "5th", 4},
                { "first option", 0},{ "second option", 1},{ "third option", 2},{ "fourth option", 3},{ "fifth option", 4},
                { "1st option", 0},{ "2nd option", 1},{ "3rd option", 2},{ "4th option", 3},{ "5th option", 4}
            };

            public static int GetChoiceKeyMatch(string key)
            {
                if (dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }

                return -1;
            }
        }
    }
}