using Parser;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotService.Dialogs
{
    public class StorySelectionState
    {
        public int ActiveStory { get; set; }

        public Section StorySection { get; set; }

        public Dictionary<string, dynamic> Stats { get; set; }

        public StringBuilder PrependBuilder { get; set; }

        public StorySelectionState(Section section, Dictionary<string, dynamic> startingStats)
        {
            StorySection = section;
            Stats = startingStats;

            PrependBuilder = new StringBuilder();
        }

        public void ApplyEffectsToStats(IEnumerable<StatEffect> effects)
        {
            foreach (var effect in effects)
            {
                if (!Stats.ContainsKey(effect.Key))
                {
                    if (effect.Value is string)
                    {
                        Stats.Add(effect.Key, 0);
                    }
                    else if (effect.Value is string[])
                    {
                        Stats.Add(effect.Key, Array.Empty<string>());
                    }
                    else
                    {
                        throw new ArgumentException($"Could not apply the effect: ${effect.Key}:${effect.Value}");
                    }
                }

                Stats.TryGetValue(effect.Key, out dynamic statsValue);

                if ((statsValue is int || statsValue is long) && effect.Value is string effectValue)
                {
                    var statsValueInt = (int)statsValue;
                    var effectInt = RollParser.Roll(effectValue);

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

        public bool AreAllConditionsMet(IEnumerable<StatEffect> conditions)
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

                    if ((statsValue is int || statsValue is long) && condition.Value is string)
                    {
                        var statsValueInt = (int)statsValue;
                        var conditionInt = RollParser.Roll(condition.Value);

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

        public bool IsAnyConditionMet(IEnumerable<StatEffect> conditions)
        {
            if (conditions == null)
            {
                return false;
            }

            foreach (var condition in conditions)
            {
                if (Stats.ContainsKey(condition.Key))
                {
                    Stats.TryGetValue(condition.Key, out dynamic statsValue);

                    if ((statsValue is int || statsValue is long) && condition.Value is string)
                    {
                        var statsValueInt = (int)statsValue;
                        var conditionInt = RollParser.Roll(condition.Value);

                        if (statsValueInt > conditionInt)
                        {
                            return true;
                        }
                    }
                    else if (statsValue is string[] statsValueArray && condition.Value is string[] conditionArray)
                    {
                        foreach (var conditionArrItem in conditionArray)
                        {
                            if (statsValueArray.FirstOrDefault(x => x == conditionArrItem) != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<Choice> GetPossibleChoices()
        {
            foreach (var choice in StorySection.Choices)
            {
                if (AreAllConditionsMet(choice.Conditions))
                {
                    yield return choice;
                }
            }
        }

        public Choice GetChoiceMetTrigger()
        {
            foreach (var choice in StorySection.Choices)
            {
                if (IsAnyConditionMet(choice.Triggers))
                {
                    return choice;
                }
            }

            return null;
        }
    }
}
