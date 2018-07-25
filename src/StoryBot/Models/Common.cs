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

            public static int GetChoiceKeyMatch(string key)
            {
                for(int i = 0; i < 5; i++)
                {
                    if(numbers[i] == key || letters[i] == key || words[i] == key)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}