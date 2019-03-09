using System.Collections.Generic;
using F23.StringSimilarity;

namespace StringTools
{
    public static class StringSimilarity
    {
        private static readonly Cosine cosine = new Cosine();
        private static readonly Jaccard jaccard = new Jaccard();
        private static readonly SorensenDice sorensenDice = new SorensenDice();

        private static readonly double threshold = 0.1;

        public static int GetIndex(string str, IEnumerable<string> strings)
        {
            var averages = new List<double>();

            foreach (var s in strings)
            {
                averages.Add((cosine.Similarity(str, s) + jaccard.Similarity(str, s) + sorensenDice.Similarity(str, s)) / 3);
            }

            int similarIndex = 0;
            for (int i = 1; i < averages.Count; i++)
            {
                if (averages[i] > averages[similarIndex])
                {
                    similarIndex = i;
                }
            }

            // If it's lower that the threshold, it doesn't match any string
            return averages[similarIndex] < threshold ? -1 : similarIndex;
        }
    }
}
