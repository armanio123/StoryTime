using AdaptiveCards;
using BotService.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotService.Dialogs
{
    public class StorySelectionDialog : ComponentDialog
    {
        private const string StoryId = "1";

        private const string StoryDialogId = "storyDialog";
        private const string EndOfStoryDialogId = "endOfStoryDialog";
        private const string OptionPromptId = "storyOption";

        private const string EndingText = "Thanks for playing.";
        private readonly List<string> DefaultOptions = new List<string>
        {
            "Continue",
            "New story"
        };

        private readonly MockApi api;
        private readonly StorySelectionAccessors accessors;

        public StorySelectionDialog(ConversationState conversationState, MockApi api)
            : base(nameof(StorySelectionDialog))
        {
            this.accessors = new StorySelectionAccessors(conversationState);

            this.api = api;

            AddDialog(new WaterfallDialog(StoryDialogId, new WaterfallStep[]{
                InitializeStateAsync,
                StoryQuestionAsync,
                StoryQuestionLoopAsync
            }));

            AddDialog(new WaterfallDialog(EndOfStoryDialogId, new WaterfallStep[]{
                StoryEndAsync
            }));

            AddDialog(new TextPrompt(OptionPromptId, ValidateOptionAsync));
        }

        private async Task<DialogTurnResult> InitializeStateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = await this.accessors.StorySelectionState.GetAsync(stepContext.Context, () => null);
            if (context == null || StringSimilarity.IsSimilar(stepContext.Context.Activity.Text, this.DefaultOptions[1])) // new story
            {
                await this.accessors.StorySelectionState.SetAsync(stepContext.Context, new StorySelectionState(api.GetStartingSection(StoryId), api.GetStartingStats(StoryId)));
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> StoryQuestionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is string optionString && !string.IsNullOrWhiteSpace(optionString))
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var state = await this.accessors.StorySelectionState.GetAsync(stepContext.Context);
                var text = $"{state.StorySection.Text} {string.Join(' ', state.StorySection.Choices.Select(x => x.Text))}";
                var options = new PromptOptions
                {
                    Prompt = CreateReply(state, stepContext.Context, text)
                };

                return await stepContext.PromptAsync(OptionPromptId, options);
            }
        }

        private async Task<DialogTurnResult> StoryQuestionLoopAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await this.accessors.StorySelectionState.GetAsync(stepContext.Context);
            state.StorySection = api.GetSectionById(StoryId, stepContext.Result.ToString());

            // Assume is the end of the story unless there's more choices.
            var dialogId = EndOfStoryDialogId;
            if (state.GetPossibleChoices().Any())
            {
                state.ActiveStory++;
                dialogId = StoryDialogId;
            }

            await this.accessors.StorySelectionState.SetAsync(stepContext.Context, state);

            return await stepContext.ReplaceDialogAsync(dialogId);
        }

        private async Task<DialogTurnResult> StoryEndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await this.accessors.StorySelectionState.GetAsync(stepContext.Context);
            var text = $"{state.StorySection.Text}\n\n{EndingText}";

            var reply = CreateReply(state, stepContext.Context, text);

            await stepContext.Context.SendActivityAsync(reply);

            // TODO: Forward to Welcome instead of finishing.
            // TODO: Add telemetry indicating a user has finished the story.
            await this.accessors.StorySelectionState.DeleteAsync(stepContext.Context, cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private async Task<bool> ValidateOptionAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var state = await this.accessors.StorySelectionState.GetAsync(promptContext.Context);

            var selectedChoice = GetSelectedChoice(state, promptContext.Recognized.Value);
            if (selectedChoice == null) // selection was not recognized, prompt the question again.
            {
                var reply = CreateReply(state, promptContext.Context, string.Join(' ', state.StorySection.Choices.Select(x => x.Text)));
                await promptContext.Context.SendActivityAsync(reply);

                return false;
            }

            promptContext.Recognized.Value = selectedChoice.SectionKey;
            state.ApplyEffectsToStats(selectedChoice.Effects);

            await this.accessors.StorySelectionState.SetAsync(promptContext.Context, state);

            return true;
        }

        private Choice GetSelectedChoice(StorySelectionState state, string input)
        {
            var cleanedInput = input?.Trim();
            if (string.IsNullOrEmpty(cleanedInput))
            {
                return null;
            }

            var availableChoices = state.GetPossibleChoices();

            // If there's a valid index, we have recognized the user input and returned the selection.
            var choiceIndex = StringSimilarity.GetIndex(cleanedInput, availableChoices.Select(x => x.Text));
            if (choiceIndex >= 0 && choiceIndex < availableChoices.Count())
            {
                return availableChoices.ElementAt(choiceIndex);
            }

            // The user input was not recognized as any valid option.
            return null;
        }

        private Activity CreateReply(StorySelectionState state, ITurnContext context, string text)
        {
            // Cortana uses isDebug. Any other channel might use something different that's why we check for null.
            bool? isDebug = ((dynamic)context.Activity.ChannelData).isDebug;

            var activity = context.Activity.CreateReply();
            activity.Speak = text;
            activity.InputHint = InputHints.IgnoringInput; // Assume there's no choices.

            activity.Text = isDebug == null || isDebug.Value
                 ? text
                 : string.Empty;

            var choices = state.GetPossibleChoices();
            if (choices.Any())
            {
                activity.InputHint = InputHints.ExpectingInput;

                var card = new AdaptiveCard("1.0")
                {
                    Speak = string.Join(' ', choices.Select(x => x.Text))
                };
                card.Actions.AddRange(choices.Select(x => new AdaptiveSubmitAction
                {
                    Title = x.Text,
                    Data = x.Text
                }));

                var attachment = new Attachment
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JObject.FromObject(card)
                };

                activity.Attachments = new List<Attachment> { attachment };
            }

            return activity;
        }
    }
}

