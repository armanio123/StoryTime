using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoryBot.Models
{
    namespace StoryTime.Shared
    {
        public static class ChoiceKeyEquivalents
        {
            static string[] numbers = { "1", "2", "3", "4", "5" };
            static string[] letters = { "a", "b", "c", "d", "e" };
            static string[] words = { "first", "second", "third", "fourth", "fifth" };

            static Dictionary<string, int> dictionary = new Dictionary<string, int>()
            {
                { "1", 0},{ "a", 0},{ "first", 0},{ "1st", 0},{ "one", 0},{ "option one", 0},{ "first option", 0},
                { "2", 1},{ "b", 1},{ "second", 1},{ "2nd", 1},{ "two", 1},{ "option two", 1},{ "second option", 1},
                { "3", 2},{ "c", 2},{ "third", 2},{ "3rd", 2},{ "three", 2},{ "option three", 2},{ "third option", 2},
                { "4", 3},{ "d", 3},{ "fourth", 3},{ "4th", 3},{ "four", 3},{ "option four", 3},{ "fourth option", 3},
                { "5", 4},{ "e", 4},{ "fifth", 4},{ "5th", 4},{ "five", 4},{ "option five", 4},{ "fifth option", 4},
            };

            public static int GetChoiceKeyMatch(string key)
            {
                if(dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }

                return -1;
            }
        }
    }
}