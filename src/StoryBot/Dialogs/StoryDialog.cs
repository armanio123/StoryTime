using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Parser.Entities;
using StoryBot.Mock;
using StoryBot.Models.StoryTime.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;

namespace StoryBot.Dialogs
{
    [Serializable]
    public class StoryDialog : IDialog<object>
    {
        private Section storySection = null;
        private Dictionary<string, dynamic> stats = null;
        MockApi api;

        public StoryDialog(ref MockApi api)
        {
            this.api = api;
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            StateClient stateClient = activity.GetStateClient();

            BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);

            string activityValue = (activity.Text?.ToLower().ToString() ?? string.Empty);

            switch (activityValue)
            {
                case "_new_story_":
                case "play again":
                    storySection = api.GetStartingSection();
                    stats = api.GetStartingStats();
                    break;
                case "_continue_":
                    string storedNode = userData.GetProperty<string>("StoryNode");
                    storySection = storedNode != null && storedNode != "" ? api.GetSectionById(storedNode) ?? api.GetStartingSection() : api.GetStartingSection();
                    stats = userData.GetProperty<Dictionary<string, dynamic>>("Stats");
                    break;
                case "_return_":

                    break;
                default:
                    if (activityValue.Length > 0)
                    {
                        var choiceSectionKey = MatchActivityValueToChoice(storySection, activityValue);

                        // If option is valid process section effects and move to next section.
                        if (choiceSectionKey != null)
                        {
                            var nextSection = api.GetSectionById(choiceSectionKey.SectionKey);

                            UpdateState(stats, choiceSectionKey.Effects);

                            if (nextSection != null)
                            {
                                storySection = nextSection;
                            }
                        }
                    }
                    break;
            }

            userData.SetProperty<Dictionary<string,dynamic>>("Stats", stats);

            userData.SetProperty<string>("StoryNode", storySection.Key);
            stateClient.BotState.SetUserData(activity.ChannelId, activity.From.Id, userData);

            Activity reply = null;

            if (storySection != null)
            {
                reply = activity.CreateReply(storySection.Text);

                List<CardAction> cardButtons = new List<CardAction>();
                List<Choice> availableChoices = new List<Choice>();
                if (storySection.Choices != null)
                {
                    foreach (var choice in storySection.Choices)
                    {
                        if (IsChoicePossible(stats, choice.Conditions))
                        {
                            availableChoices.Add(choice);
                            cardButtons.Add(new CardAction()
                            {
                                Title = choice.Text,
                                Text = choice.Text,
                                DisplayText = choice.Text,
                                Value = choice.SectionKey,
                                Type = ActionTypes.PostBack
                            });
                        }
                    }
                }

                reply.Speak = BuildSpeakText(storySection.Text, availableChoices);

                // If we have reached the end of the story or if there are not enough choices remaining prompt player
                // to start the story again.
                if (storySection.Choices == null || availableChoices.Count == 0)
                {
                    reply.Speak += " You have reached the end of the story, you can say, Play again!, to play the story again.";

                    cardButtons.Add(new CardAction()
                    {
                        Title = "Play again!",
                        Text = "Play again!",
                        DisplayText = "Play again!",
                        Value = "_new_story_",
                        Type = ActionTypes.PostBack
                    });
                }

                var heroCard = new HeroCard()
                {
                    Buttons = cardButtons
                };

                reply.Attachments.Add(heroCard.ToAttachment());
            }

            reply.Type = ActivityTypes.Message;
            reply.TextFormat = TextFormatTypes.Plain;

            await context.PostAsync(reply);

            context.Wait(MessageReceivedAsync);
        }

        private string BuildSpeakText(string text, IEnumerable<Choice> choices)
        {
            string result = text;
            if (choices != null && choices.Count() > 0)
            {
                result += " You have the following options, you can...";
                int count = 1;
                foreach (var option in choices)
                {
                    result += string.Format(" {0}) {1}", count, option.Text);
                    count++;
                }
            }

            return result;
        }

        Choice MatchActivityValueToChoice(Section storySection, string activityValue)
        {
            foreach(var choice in storySection.Choices)
            {
                if(choice.SectionKey.ToLower() == activityValue)
                {
                    return choice;
                }
            }

            int choiceIndex = ChoiceKeyEquivalents.GetChoiceKeyMatch(activityValue);

            if (choiceIndex > -1 && choiceIndex < storySection.Choices.Count())
            {
                return storySection.Choices.ElementAt(choiceIndex);
            }

            return null;
        }

        bool IsChoicePossible(Dictionary<string, dynamic> state, IEnumerable<StatEffect> conditions)
        {
            if(conditions == null)
            {
                return true;
            }

            foreach(var condition in conditions)
            {
                if (stats.ContainsKey(condition.Key))
                {
                    dynamic statsValue = null;
                    stats.TryGetValue(condition.Key, out statsValue);

                    
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
                            if(statsValueArray.FirstOrDefault(x => x == conditionArrItem) == null)
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

        void UpdateState(Dictionary<string, dynamic> state, IEnumerable<StatEffect> effects)
        {
            foreach(var effect in effects)
            {
                if (stats.ContainsKey(effect.Key))
                {
                    dynamic statsValue = null;
                    stats.TryGetValue(effect.Key, out statsValue);

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
    }
}