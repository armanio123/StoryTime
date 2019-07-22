using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotService.Dialogs
{
    public class StorySelectionState
    {
        public int ActiveStory { get; set; }

        public Section StorySection { get; set; }

        public Dictionary<string, dynamic> Stats { get; set; }

        public StorySelectionState(Section section, Dictionary<string, dynamic> startingStats)
        {
            StorySection = section;
            Stats = startingStats;
        }

        public void ApplyEffectsToStats(IEnumerable<StatEffect> effects)
        {
            foreach (var effect in effects)
            {
                if (Stats.ContainsKey(effect.Key))
                {
                    Stats.TryGetValue(effect.Key, out dynamic statsValue);

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
                        Stats[effect.Key] = statsValueInt;
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

                                    Stats[effect.Key] = tempStatsList.ToArray();
                                    break;
                                }
                            case EffectType.RemoveOrDontHave:
                                {
                                    var tempStatsList = statsValueArray.ToList();

                                    foreach (var effectArrItem in effectArray)
                                    {
                                        tempStatsList.Remove(effectArrItem);
                                    }

                                    Stats[effect.Key] = tempStatsList.ToArray();
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
                if (Stats.ContainsKey(condition.Key))
                {
                    Stats.TryGetValue(condition.Key, out dynamic statsValue);


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

        public IEnumerable<Choice> GetPossibleChoices()
        {
            foreach (var choice in StorySection.Choices)
            {
                if (IsChoicePossible(choice.Conditions))
                {
                    yield return choice;
                }
            }
        }
    }
}
