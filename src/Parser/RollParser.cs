using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Parser
{
    public static class RollParser
    {
        internal static readonly string Pattern = @"(\d+)(?:d(\d+))?"; // Group 1 = Fixed amount or number of rolls, Group 2 = Max value, Group 3 = Roll key

        private static readonly Regex RollRegex = new Regex(Pattern, RegexOptions.IgnoreCase);

        public static int Roll(string pattern)
        {
            var match = RollParser.RollRegex.Match(pattern);

            if (match.Success)
            {
                var roll = int.Parse(match.Groups[1].Value);
                var maxValue = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? -1 : int.Parse(match.Groups[2].Value);

                return maxValue == -1 ? roll : Roll(roll, maxValue);
            }
            else
            {
                throw new ArgumentException("Unable to parse Roll");
            }
        }

        public static int Roll(int roll, int maxValue)
        {
            var random = new Random();
            int result = 0;
            for (int i = 0; i < roll; i++)
            {
                result += random.Next(1, maxValue);
            }

            return result;
        }
    }
}
