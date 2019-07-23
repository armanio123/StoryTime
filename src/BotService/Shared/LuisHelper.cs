// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BotService.Shared
{
    public static class LuisHelper
    {
        public static async Task ExecuteLuisQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // Create the LUIS settings from configuration.
                var luisApplication = new LuisApplication(
                    configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    "https://" + configuration["LuisAPIHostName"]
                );

                var recognizer = new LuisRecognizer(luisApplication);

                // The actual call to LUIS
                var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);

                var (intent, score) = recognizerResult.GetTopScoringIntent();
            }
            catch (Exception e)
            {
                logger.LogWarning($"LUIS Exception: {e.Message} Check your LUIS configuration.");
            }
        }
    }
}
