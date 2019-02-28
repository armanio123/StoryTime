using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StoryBot.Mock;
using StringTools;

namespace StoryBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public MockApi api;
        private readonly List<string> options = new List<string>
        {
            "continue",
            "start story"
        };

        public async Task StartAsync(IDialogContext context)
        {
            api = new MockApi();

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            StateClient stateClient = activity.GetStateClient();
            //stateClient.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
            var storyNodeIndex = StringSimilarity.GetIndex(activity.Text, this.options);
            var storyNode = storyNodeIndex != -1 ? options[storyNodeIndex] : string.Empty;

            BotData userData = stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);
            string storedStoryNode = userData.GetProperty<string>("StoryNode");

            Activity reply;

            switch (storyNode)
            {
                case "continue":
                    activity.Text = "_continue_";
                    await context.Forward(new Dialogs.StoryDialog(ref api), this.ResumeAfterNewOrderDialog, activity, CancellationToken.None);
                    break;
                case "start story":
                    activity.Text = "_new_story_";
                    await context.Forward(new Dialogs.StoryDialog(ref api), this.ResumeAfterNewOrderDialog, activity, CancellationToken.None);
                    break;
                default:
                    string text = "Welcome to an Adventure!";
                    reply = activity.CreateReply(text);
                    reply.Speak = storedStoryNode != null ? "Welcome to an Adventure! You can say, continue!, or say, start story!" : "Welcome to an Adventure! You can say, start story!";

                    List<CardAction> cardButtons = new List<CardAction>();

                    if (storedStoryNode != null)
                    {
                        cardButtons.Add(new CardAction()
                        {
                            Title = "Continue",
                            Value = "continue",
                            Type = "imBack"
                        });
                    }

                    cardButtons.Add(new CardAction()
                    {
                        Title = "Start Story",
                        Value = "start story",
                        Type = "imBack"
                    });

                    var heroCard = new HeroCard()
                    {
                        Buttons = cardButtons
                    };

                    reply.Attachments.Add(heroCard.ToAttachment());

                    reply.InputHint = InputHints.ExpectingInput;
                    reply.Type = ActivityTypes.Message;
                    reply.TextFormat = TextFormatTypes.Plain;

                    await context.PostAsync(reply);

                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task ResumeAfterNewOrderDialog(IDialogContext context, IAwaitable<object> result)
        {
            // Store the value that NewOrderDialog returned. 
            // (At this point, new order dialog has finished and returned some value to use within the root dialog.)
            var resultFromNewOrder = await result;

            await context.PostAsync($"New order dialog just told me this: {resultFromNewOrder}");

            // Again, wait for the next message from the user.
            context.Wait(this.MessageReceivedAsync);
        }
    }
}