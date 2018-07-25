using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Parser.Entities;
using StoryBot.Mock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoryBot.Dialogs
{
    [Serializable]
    public class StoryDialog : IDialog<object>
    {
        private Section storySection = null;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            MockApi api = new MockApi();

            StateClient stateClient = activity.GetStateClient();

            BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);

            string activityValue = (activity.Text.ToString() ?? string.Empty);
            Dictionary<string, dynamic> stats = null;

            switch (activityValue)
            {
                case "_new_story_":
                    storySection = api.GetStartingSection();
                    stats = api.GetStartingStats();
                    break;
                case "_continue_":
                    string storedNode = userData.GetProperty<string>("StoryNode");
                    storySection = storedNode != null && storedNode != "" ? api.GetSectionById(storedNode) ?? api.GetStartingSection() : api.GetStartingSection();
                    break;
                case "_return_":

                    break;
                default:
                    if (activityValue.Length > 0)
                    {
                        storySection = api.GetSectionById(activityValue) ?? storySection;
                    }
                    break;
            }

            UpdateState(stats, null);

            userData.SetProperty<Dictionary<string, dynamic>>("Stats", stats);

            userData.SetProperty<string>("StoryNode", storySection.Key);
            stateClient.BotState.SetUserData(activity.ChannelId, activity.From.Id, userData);

            Activity reply = null;

            if (storySection != null)
            {
                reply = activity.CreateReply(storySection.Text);

                reply.Speak = BuildSpeakText(storySection.Text, storySection.Choices);

                List<CardAction> cardButtons = new List<CardAction>();

                if (storySection.Choices != null)
                {
                    foreach (var choice in storySection.Choices)
                    {
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
                else
                {
                    reply.Speak = " You have reached the end of the story, you can say, Play again!, to play the story again.";

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
            if (choices != null)
            {
                int count = 1;
                foreach (var option in choices)
                {
                    result += string.Format(" {0}) {1}", count, option);
                    count++;
                }
            }

            return result;
        }

        private void UpdateState(Dictionary<string, dynamic> state, IEnumerable<StatEffect> effects)
        {

        }
    }
}