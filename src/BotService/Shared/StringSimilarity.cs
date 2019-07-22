using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotService.Shared
{
    public static class StringSimilarity
    {
        private static readonly Cosine cosine = new Cosine();
        private static readonly Jaccard jaccard = new Jaccard();
        private static readonly SorensenDice sorensenDice = new SorensenDice();

        private static readonly double threshold = 0.1;

        public static int GetIndex(string str, IEnumerable<string> strings)
        {
            double maxSimilarity = -1;
            int index = -1;

            var stringList = strings.ToList();
            for (int i = 0; i < stringList.Count; i++)
            {
                var s = stringList[i];
                if(IsSimilar(str, s, out var similarity) && maxSimilarity < similarity)
                { 
                    maxSimilarity = similarity;
                    index = i;
                }

            }

            // TODO: Add telemetry to check amount of failed queries.
            return index;
        }

        public static bool IsSimilar(string str1, string str2, out double similarityAverage)
        {
            similarityAverage = GetSimilarityAverage(str1, str2);

            // It matches a string if it's greater than the threshold.
            return similarityAverage > threshold;
        }

        public static bool IsSimilar(string str1, string str2)
        {
            return IsSimilar(str1, str2, out _);
        }

        public static double GetSimilarityAverage(string str1, string str2)
        {
            return (cosine.Similarity(str1, str2) + jaccard.Similarity(str1, str2) + sorensenDice.Similarity(str1, str2)) / 3;
        }
    }
}
