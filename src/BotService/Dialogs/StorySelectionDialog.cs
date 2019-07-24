using AdaptiveCards;
using BotService.Shared;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotService.Dialogs
{
    public class StorySelectionDialog : ComponentDialog
    {
        private const string StoryId = "4";

        private const string StoryDialogId = "storyDialog";
        private const string EndOfStoryDialogId = "endOfStoryDialog";
        private const string OptionPromptId = "storyOption";

        private const string EndingText = "Thanks for playing.";
        private readonly List<string> DefaultOptions = new List<string>
        {
            "Continue",
            "New story"
        };

        private MockApi api;
        private readonly StorySelectionAccessors accessors;

        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;

        public StorySelectionDialog(
            IConfiguration configuration,
            ILogger<StorySelectionDialog> logger,
            ConversationState conversationState)
            : base(nameof(StorySelectionDialog))
        {
            this._configuration = configuration;
            this._logger = logger;

            this.accessors = new StorySelectionAccessors(conversationState);

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
            this.api = new MockApi();

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
                var text = $"{state.StorySection.Text}";
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

            while (string.IsNullOrWhiteSpace(state.StorySection.Text)) // Is a linked state
            {
                state.StorySection = api.GetSectionById(StoryId, state.GetPossibleChoices().Single().SectionKey);
            }

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

            var selectedChoice = await GetSelectedChoice(state, promptContext, cancellationToken);
            if (selectedChoice == null) // selection was not recognized, prompt the question again.
            {
                var reply = CreateReply(state, promptContext.Context, "I don't understand what you are trying to do, please try again. You may also ask me to repeat the description.");
                await promptContext.Context.SendActivityAsync(reply);

                return false;
            }

            promptContext.Recognized.Value = selectedChoice.SectionKey;
            state.ApplyEffectsToStats(selectedChoice.Effects);

            await this.accessors.StorySelectionState.SetAsync(promptContext.Context, state);

            return true;
        }

        private async Task<Choice> GetSelectedChoice(StorySelectionState state, PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var cleanedInput = promptContext.Recognized.Value?.Trim();
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

            // If there's no choice, use LUIS to try to get the best result.
            var luisResult = await LuisHelper.ExecuteLuisQuery(_configuration, _logger, promptContext.Context, cancellationToken);
            
            // aaward log testing
            _logger.LogInformation($"LUIS result: {luisResult}");
            var selection = availableChoices.SingleOrDefault(x => string.Equals(x.Text, luisResult, StringComparison.OrdinalIgnoreCase));
            //_logger.LogInformation($"Thing one: {x.Text}");
            _logger.LogInformation($"Selection: {selection}");
            _logger.LogInformation($"Available : {availableChoices}");
            
            return availableChoices.SingleOrDefault(x => string.Equals(x.Text, luisResult, StringComparison.OrdinalIgnoreCase));
        }

        private Activity CreateReply(StorySelectionState state, ITurnContext context, string text)
        {
            // Cortana uses isDebug. Any other channel might use something different that's why we check for null.
            bool? isDebug = ((dynamic)context.Activity.ChannelData).isDebug;

            var activity = context.Activity.CreateReply();
            activity.Speak = text;
            activity.InputHint = InputHints.ExpectingInput;

            activity.Text = isDebug == null || isDebug.Value
                 ? text
                 : string.Empty;

            activity.Properties = JObject.FromObject(new { stats = state.Stats });

            return activity;
        }
    }
}

