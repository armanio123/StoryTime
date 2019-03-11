using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Parser.Entities;
using StringTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs.StorySelection
{
    public class StorySelectionDialog : ComponentDialog
    {
        static string StoryDialog = "storyDialog";
        static string EndOfStoryDialog = "endOfStoryDialog";
        static string OptionPrompt = "storyOption";
        //private Section storySection = null;
        MockApi api;

        public StorySelectionDialog(IStatePropertyAccessor<StorySelectionState> userProfileStateAccessor, ILoggerFactory loggerFactory, MockApi api)
            : base(nameof(StorySelectionDialog))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));
            this.api = api;
            //storySection = api.GetStartingSection("0");

            AddDialog(new WaterfallDialog(StoryDialog, new WaterfallStep[]{
                InitializeStateAsync,
                StoryQuestionAsync,
                StoryQuestionLoopAsync
            }));

            AddDialog(new WaterfallDialog(EndOfStoryDialog, new WaterfallStep[]{
                StoryEndAsync
            }));

            AddDialog(new TextPrompt(OptionPrompt, ValidateOption));
        }

        public IStatePropertyAccessor<StorySelectionState> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var storySelectionState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (storySelectionState == null)
            {
                //var storySelectionStateOpt = stepContext.Options as StorySelectionState;
                //if (storySelectionStateOpt != null)
                //{
                //    await UserProfileAccessor.SetAsync(stepContext.Context, storySelectionState);
                //}
                //else
                //{
                    await UserProfileAccessor.SetAsync(stepContext.Context, new StorySelectionState(api.GetStartingSection("0"), api.GetStartingStats("0")));
                //}
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> StoryQuestionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var optionString = stepContext.Result as string;
            if (optionString != null && optionString != "")
            {
                return await stepContext.NextAsync();
            }
            else
            {
                var state = await UserProfileAccessor.GetAsync(stepContext.Context);

                // prompt for name, if missing
                var opts = new PromptOptions
                {
                    Prompt = BuildSectionQuestion(state, stepContext.Context, state.storySection.Text, string.Format("{0} {1}", state.storySection.Text, ConcatenateSectionChoices(state.storySection.Choices)), state.storySection.Choices)
                };
                return await stepContext.PromptAsync(OptionPrompt, opts);
            }
        }

        private async Task<DialogTurnResult> StoryQuestionLoopAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);
            state.storySection = api.GetSectionById(stepContext.Result.ToString(), "0");
            await UserProfileAccessor.SetAsync(stepContext.Context, state);

            if (state.storySection.Choices.Count() == 0)
            {
                return await stepContext.ReplaceDialogAsync(EndOfStoryDialog);
            }
            else
            {
                var storySelectionState = await UserProfileAccessor.GetAsync(stepContext.Context);
                storySelectionState.activeStory++;
                await UserProfileAccessor.SetAsync(stepContext.Context, storySelectionState);

                return await stepContext.ReplaceDialogAsync(StoryDialog);
            }
        }

        private async Task<DialogTurnResult> StoryEndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(stepContext.Context);

            await stepContext.Context.SendActivityAsync(new Activity
            {
                Text = state.storySection.Text,
                Speak = state.storySection.Text,
                Type = "message"
            });

            state.storySection = api.GetStartingSection("0");
            state.stats = api.GetStartingStats("0");
            await UserProfileAccessor.SetAsync(stepContext.Context, state);

            return await stepContext.ReplaceDialogAsync(StoryDialog);
        }

        private async Task<bool> ValidateOption(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var state = await UserProfileAccessor.GetAsync(promptContext.Context);
            var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;

            Choice selectedChoice = MatchActivityValueToChoice(state, value);

            if (selectedChoice != null)
            {
                promptContext.Recognized.Value = selectedChoice.SectionKey;
                state.UpdateStats(selectedChoice.Effects);
                await UserProfileAccessor.SetAsync(promptContext.Context, state);
                return true;
            }
            else
            {
                var reply = BuildSectionQuestion(state, promptContext.Context, "", ConcatenateSectionChoices(state.storySection.Choices), state.storySection.Choices);
                await promptContext.Context.SendActivityAsync(reply);

                return false;
            }
        }

        private string ConcatenateSectionChoices(IEnumerable<Choice> choices)
        {
            string choicesText = "";
            foreach(var choice in choices)
            {
                choicesText += (choicesText == "" ? "" : " ") + choice.Text;
            }

            return choicesText;
        }

        private Choice MatchActivityValueToChoice(StorySelectionState state, string activityValue)
        {
            List<Choice> availableChoices = new List<Choice>();

            foreach (var choice in state.storySection.Choices)
            {
                if (state.IsChoicePossible(choice.Conditions))
                {
                    availableChoices.Add(choice);
                }
            }

            string activityLowerCase = activityValue.ToLower();

            foreach (var choice in availableChoices)
            {
                if (choice.SectionKey.ToLower() == activityLowerCase || choice.Text.ToLower() == activityLowerCase || choice.Text.ToLower() == activityLowerCase + ".")
                {
                    return choice;
                }
            }

            List<string> choicesText = new List<string>();
            foreach(Choice choice in availableChoices)
            {
                choicesText.Add(choice.Text);
            }

            int choiceIndex = StringSimilarity.GetIndex(activityLowerCase, choicesText);

            if (choiceIndex > -1 && choiceIndex < availableChoices.Count())
            {
                return availableChoices.ElementAt(choiceIndex);
            }

            return null;
        }

        Activity BuildSectionQuestion(StorySelectionState state, ITurnContext context, string text, string speak, IEnumerable<Choice> choices)
        {
            var activity = context.Activity.CreateReply(text);
            activity.Speak = speak;
            activity.InputHint = InputHints.ExpectingInput;

            List<CardAction> cardButtons = new List<CardAction>();
            foreach (var choice in choices)
            {
                if (state.IsChoicePossible(choice.Conditions))
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

            var heroCard = new HeroCard()
            {
                Buttons = cardButtons
            };

            activity.Attachments.Add(heroCard.ToAttachment());

            return activity;
        }
    }
}
