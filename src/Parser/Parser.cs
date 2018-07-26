﻿using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Newtonsoft.Json;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parser
{
    public class MarkdownStoryParser
    {
        private static readonly string statKeyPattern = @"^ *?([^: ]+) *?:"; // Group 1 = key name.
        private static readonly string counterStatPattern = $@"{statKeyPattern} *?([-+])?(\d+) *?$"; // Group 1 = key name. Group 2 = Sign, Group 3 = Value. 
        private static readonly string flagStatPattern = $@"{statKeyPattern} *?([-+])?(\[.+\]) *?$"; // Group 1 = key name. Group 2 = Sign, Group 3 = Array.

        private readonly Regex counterStatRegex = new Regex(counterStatPattern, RegexOptions.IgnoreCase);
        private readonly Regex flagStatRegex = new Regex(flagStatPattern, RegexOptions.IgnoreCase);

        public Story Parse(string markdownString)
        {
            var markdownDocument = Markdown.Parse(markdownString);

            return Parse(markdownDocument);
        }

        public Story Parse(MarkdownDocument markdownDocument)
        {
            Story story = new Story
            {
                Sections = new Dictionary<string, Section>(),
                Stats = new Dictionary<string, dynamic>(),
                Title = null
            };

            for (int i = 0; i < markdownDocument.Count; i++)
            {
                if (!(markdownDocument[i] is HeadingBlock headingBlock))
                {
                    throw new Exception("The Story doesn't contain a Title, Section or Stats.");
                }

                string headingContent = GetBlockContent(headingBlock);

                switch (headingBlock.Level)
                {
                    // Heading 1 is reserverd for Title
                    case 1:
                        // TODO: Validate if more than on title is found.
                        story.Title = story.Title ?? headingContent;

                        string author = GetBlockContent(markdownDocument, i + 1);

                        if ( author == null )
                        {
                            story.Author = "by Anonymous";
                        }
                        else
                        {
                            story.Author = author;
                            i++;
                        }

                        break;
                    // Either Stats or Content regardless of heading
                    default:
                        bool isStats = string.Equals("stats", headingContent, StringComparison.OrdinalIgnoreCase);

                        i++;

                        if (isStats)
                        {
                            // TODO: Validate if the stats are already found.
                            AddStats(story.Stats, ParseStats(markdownDocument, i));
                        }
                        else
                        {
                            var paragraphs = ParseContent(markdownDocument, i);
                            var content = string.Join(string.Empty, paragraphs);

                            i += paragraphs.Count() - 1;

                            var nextBlock = i + 2; // Skip title and place index in the Choices.
                            var choices = ParseChoices(markdownDocument, nextBlock);
                            i = choices.Any() ? nextBlock : i; // Move index if there are any choices; otherwise keep it the same.

                            Section section = new Section
                            {
                                Key = headingContent,
                                Choices = choices,
                                Text = content
                            };

                            if (!TryAddDictionary(story.Sections, headingContent, section))
                            {
                                throw new Exception("Section title already found. Use a different name.");
                            }
                        }
                        break;
                }
            }

            return story;
        }

        private bool TryAddDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);

            return true;
        }

        private IEnumerable<string> ParseContent(MarkdownDocument markdownDocument, int i)
        {
            // Concat all paragraphs, skip the blocks already parsed.
            var paragraphs = markdownDocument.Skip(i).TakeWhile(x => x is ParagraphBlock).Select(GetBlockContent);

            if (!paragraphs.Any())
            {
                throw new Exception("Error parsing a Section. No text was found."); //TODO: Improve error handling
            }

            return paragraphs;
        }

        private Dictionary<string, dynamic> ParseStats(MarkdownDocument markdownDocument, int i)
        {
            if (markdownDocument.Count() < i || !(markdownDocument[i] is ListBlock listBlock))
            {
                throw new Exception("No Stats found. After a Stats heading a list of the Stats should be present.");
            }

            var stats = new Dictionary<string, dynamic>();

            foreach (ListItemBlock listItemBlock in listBlock)
            {
                if (!(listItemBlock[0] is ParagraphBlock paragraphBlock))
                {
                    throw new Exception("Error parsing the Stats. An empty item is not allowed.");
                }

                string content = GetBlockContent(paragraphBlock);

                var (key, value, _) = GetStat(content);

                stats.Add(key, value);
            }

            return stats;
        }

        private List<Choice> ParseChoices(MarkdownDocument markdownDocument, int i)
        {
            var choices = new List<Choice>();

            if (markdownDocument.Count() < i)
            {
                return choices;
            }

            if (!(markdownDocument[i] is ListBlock listBlock))
            {
                return choices;
            }

            foreach (ListItemBlock listItemBlock in listBlock)
            {
                choices.Add(ParseChoice(listItemBlock));
            }

            return choices;
        }

        private Choice ParseChoice(ListItemBlock listItemBlock)
        {
            if (!(listItemBlock[0] is ParagraphBlock paragraphBlock))
            {
                throw new Exception("Unable to parse the Choice. No text found.");
            }

            string text = GetBlockContent(paragraphBlock);
            string sectionKey = paragraphBlock.Inline.Descendants<LinkInline>().FirstOrDefault().Url;

            var (conditions, effects) = ParseStatEffects(listItemBlock);

            return new Choice
            {
                Conditions = conditions,
                Effects = effects,
                Text = text,
                SectionKey = sectionKey
            };
        }

        private (List<StatEffect> conditions, List<StatEffect> effects) ParseStatEffects(ListItemBlock listItemBlock)
        {
            var conditions = new List<StatEffect>();
            var effects = new List<StatEffect>();

            // No effects or conditions.
            if (listItemBlock.Count < 1 || !(listItemBlock[1] is ListBlock listBlock))
            {
                return (conditions, effects);
            }

            foreach (ParagraphBlock paragraphBlock in listBlock.Descendants<ParagraphBlock>())
            {
                string content = GetBlockContent(paragraphBlock);

                (string key, dynamic value, EffectType effectType) = GetStat(content);

                if (paragraphBlock.Inline.FirstChild is EmphasisInline) // Is a Condition
                {
                    conditions.Add(new StatEffect
                    {
                        EffectType = effectType,
                        Key = key,
                        Value = value
                    });
                }
                else
                {
                    effects.Add(new StatEffect
                    {
                        EffectType = effectType,
                        Key = key,
                        Value = value
                    });
                }
            }

            return (conditions, effects);
        }

        private void AddStats(Dictionary<string, dynamic> stats, Dictionary<string, dynamic> newStats)
        {
            foreach (var keyValuePair in newStats)
            {
                if (!TryAddDictionary(stats, keyValuePair.Key, keyValuePair.Value))
                {
                    throw new Exception("Error parsing the Stats. The Stat name already exists.");
                }
            }
        }

        private (string key, dynamic value, EffectType effectType) GetStat(string content)
        {
            if (TryGetCounterStat(content, out string key, out dynamic value, out EffectType effectType) || TryGetFlagStat(content, out key, out value, out effectType))
            {
                return (key, value, effectType);
            }

            throw new Exception("Unable to parse Stat.");
        }

        private bool TryGetFlagStat(string content, out string key, out dynamic value, out EffectType effectType)
        {
            key = null;
            value = null;
            effectType = EffectType.None;

            Match match = flagStatRegex.Match(content);
            if (match.Success)
            {
                key = match.Groups[1].Value;
                value = JsonConvert.DeserializeObject<string[]>(match.Groups[3].Value);
                effectType = GetEffectType(match.Groups[2].Value);
            }

            return match.Success;
        }

        private bool TryGetCounterStat(string content, out string key, out dynamic value, out EffectType effectType)
        {
            key = null;
            value = null;
            effectType = EffectType.None;

            Match match = counterStatRegex.Match(content);

            if (match.Success)
            {
                key = match.Groups[1].Value;
                value = int.Parse(match.Groups[3].Value);
                effectType = GetEffectType(match.Groups[2].Value);
            }

            return match.Success;
        }

        private EffectType GetEffectType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return EffectType.Set;
            }

            switch (value)
            {
                case "+": return EffectType.AddOrHave;
                case "-": return EffectType.RemoveOrDontHave;
                default: return EffectType.None; // TODO: Validate invalid characters instead.
            }
        }

        private string GetBlockContent(MarkdownDocument markdownDocument, int i)
        {
            if(markdownDocument.Count() > i)
            {
                return GetBlockContent(markdownDocument[i]);
            }

            return null;
        }

        private string GetBlockContent(Block block)
        {
            if (block is HeadingBlock headingBlock)
            {
                return headingBlock.Inline.Descendants<LiteralInline>().FirstOrDefault().Content.ToString();
            }
            if (block is ParagraphBlock paragraphBlock)
            {
                return string.Join(string.Empty, paragraphBlock.Inline.Descendants<LiteralInline>().Select(x => x.Content.ToString()));
            }

            return null;
        }
    }
}
