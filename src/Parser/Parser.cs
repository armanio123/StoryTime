using Markdig;
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
        private static readonly string counterStatPattern = $@"{statKeyPattern} *?([-+]?\d+) *?$"; // Group 1 = key name. Group 2 = Value, including sign. 
        private static readonly string flagStatPattern = $@"{statKeyPattern} *?([-+])?(\[.+\]) *?$"; // Group 1 = key name. Group 2 = Sign, Group 3 = Array.

        private readonly Regex counterStatRegex = new Regex(counterStatPattern, RegexOptions.IgnoreCase);
        private readonly Regex flagStatRegex = new Regex(flagStatPattern, RegexOptions.IgnoreCase);

        public Story Parse(string markdownString)
        {
            var markdownDocument = Markdown.Parse(markdownString);

            return ParseStory(markdownDocument);
        }

        public Story ParseStory(MarkdownDocument markdownDocument)
        {
            string title = null;
            var stats = new Dictionary<string, dynamic>();
            var sections = new Dictionary<string, Section>();

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
                        title = title ?? headingContent;
                        break;
                    // Either Stats or Content regardless of heading
                    default:
                        bool isStats = string.Equals("stats", headingContent, StringComparison.OrdinalIgnoreCase);

                        if (isStats)
                        {
                            // TODO: Validate if the stats are already found.
                            i++;
                            AddStats(stats, ParseStats(markdownDocument, i));
                        }
                        else
                        {
                            i++;

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

                            if (!sections.TryAdd(headingContent, section))
                            {
                                throw new Exception("Section title already found. Use a different name.");
                            }
                        }
                        break;
                }
            }

            return new Story
            {
                Sections = sections,
                Stats = stats,
                Title = title
            };
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

                var (key, value) = GetStat(content);

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

                if (TryGetCounterState(content, out string key, out dynamic value, out EffectType effectType) || TryGetFlagStat(content, out key, out value, out effectType))
                {
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
                else
                {
                    throw new Exception("Error when parsing an Stat Effect.");
                }
            }

            return (conditions, effects);
        }

        private void AddStats(Dictionary<string, dynamic> stats, Dictionary<string, dynamic> newStats)
        {
            foreach (var keyValuePair in newStats)
            {
                if (!stats.TryAdd(keyValuePair.Key, keyValuePair.Value))
                {
                    throw new Exception("Error parsing the Stats. The Stat name already exists.");
                }
            }
        }

        private (string key, dynamic value) GetStat(string content)
        {
            if (TryGetCounterState(content, out string key, out dynamic value, out EffectType _) || TryGetFlagStat(content, out key, out value, out _))
            {
                return (key, value);
            }

            throw new Exception("Unable to parse the Stats.");
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
                effectType = match.Groups[2].Value == "-" ? EffectType.RemoveOrDontHave : EffectType.AddOrHave;
            }

            return match.Success;
        }

        private bool TryGetCounterState(string content, out string key, out dynamic value, out EffectType effectType)
        {
            key = null;
            value = null;
            effectType = EffectType.None;

            Match match = counterStatRegex.Match(content);

            if (match.Success)
            {
                key = match.Groups[1].Value;
                value = int.Parse(match.Groups[2].Value);
                effectType = value < 0 ? EffectType.RemoveOrDontHave : EffectType.AddOrHave;
            }

            return match.Success;
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
