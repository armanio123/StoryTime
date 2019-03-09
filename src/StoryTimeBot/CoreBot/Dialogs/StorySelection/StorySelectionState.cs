using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Dialogs.StorySelection
{
    public class StorySelectionState
    {
        public int activeStory { get; set; }
        public int option { get; set; }
        public Section storySection { get; set; }
        public Dictionary<string, dynamic> stats { get; set; }

        public StorySelectionState(Section section, Dictionary<string, dynamic> startingStats)
        {
            option = -1;
            storySection = section;
            stats = startingStats;
        }

        public void UpdateStats(IEnumerable<StatEffect> effects)
        {
            foreach (var effect in effects)
            {
                if (stats.ContainsKey(effect.Key))
                {
                    stats.TryGetValue(effect.Key, out dynamic statsValue);

                    if ((statsValue is int || statsValue is long) && effect.Value is int effectInt)
                    {
                        int statsValueInt = (int)statsValue;
                        switch (effect.EffectType)
                        {
                            case EffectType.None:
                                statsValueInt = effectInt;
                                break;
                            case EffectType.AddOrHave:
                                statsValueInt += effectInt;
                                break;
                            case EffectType.RemoveOrDontHave:
                                statsValueInt -= effectInt;
                                break;
                        }
                        stats[effect.Key] = statsValueInt;
                    }
                    else if (statsValue is string[] statsValueArray && effect.Value is string[] effectArray)
                    {
                        switch (effect.EffectType)
                        {
                            case EffectType.AddOrHave:
                                {
                                    var tempStatsList = statsValueArray.ToList();

                                    foreach (var effectArrItem in effectArray)
                                    {
                                        // Validate Key doesn't exist before inserting it.
                                        if (tempStatsList.FirstOrDefault(x => x == effectArrItem) == null)
                                        {
                                            tempStatsList.Add(effectArrItem);
                                        }
                                    }

                                    stats[effect.Key] = tempStatsList.ToArray();
                                    break;
                                }
                            case EffectType.RemoveOrDontHave:
                                {
                                    var tempStatsList = statsValueArray.ToList();

                                    foreach (var effectArrItem in effectArray)
                                    {
                                        tempStatsList.Remove(effectArrItem);
                                    }

                                    stats[effect.Key] = tempStatsList.ToArray();
                                    break;
                                }
                        }
                    }
                }
            }
        }

        public bool IsChoicePossible(IEnumerable<StatEffect> conditions)
        {
            if (conditions == null)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                if (stats.ContainsKey(condition.Key))
                {
                    stats.TryGetValue(condition.Key, out dynamic statsValue);


                    if ((statsValue is int || statsValue is long) && condition.Value is int conditionInt)
                    {
                        int statsValueInt = (int)statsValue;
                        if (statsValueInt < conditionInt)
                        {
                            return false;
                        }
                    }
                    else if (statsValue is string[] statsValueArray && condition.Value is string[] conditionArray)
                    {
                        foreach (var conditionArrItem in conditionArray)
                        {
                            if (statsValueArray.FirstOrDefault(x => x == conditionArrItem) == null)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
