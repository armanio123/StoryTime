using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Newtonsoft.Json;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Parser
{
    public class MarkdownStoryParser
    {
        private static readonly string statKeyPattern = @"^ *?([^: ]+) *?:"; // Group 1 = key name.

        private static readonly string counterStatPattern = $@"{statKeyPattern} *?([-+])?({RollParser.Pattern}) *?$"; // Group 1 = key name. Group 2 = Sign, Group 3 = Value. 
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
                Author = string.Empty,
                Sections = new Dictionary<string, Section>(),
                Stats = new Dictionary<string, dynamic>(),
                Title = string.Empty
            };

            foreach (var headingBlock in markdownDocument.Descendants<HeadingBlock>())
            {
                var nextBlockIndex = markdownDocument.IndexOf(headingBlock) + 1;

                string headingContent = GetBlockContent(headingBlock);

                if (string.Equals(headingContent, "actions", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                switch (headingBlock.Level)
                {
                    // Heading 1 is reserverd for Title
                    case 1:
                        // TODO: Validate if more than on title is found.
                        story.Title = string.IsNullOrWhiteSpace(story.Title) ? headingContent : story.Title;

                        string author = GetBlockContent<ParagraphBlock>(markdownDocument, nextBlockIndex);

                        if (!string.IsNullOrWhiteSpace(author))
                        {
                            story.Author = author;
                        }

                        break;
                    // Either Stats or Content regardless of heading
                    default:
                        bool isStats = string.Equals("stats", headingContent, StringComparison.OrdinalIgnoreCase);

                        if (isStats)
                        {
                            // TODO: Validate if the stats are already found.
                            AddStats(story.Stats, ParseStats(markdownDocument, nextBlockIndex));
                        }
                        else
                        {
                            var paragraphs = ParseContent(markdownDocument, nextBlockIndex);
                            string content = CleanContent(paragraphs);

                            var nextBlock = SkipComments(markdownDocument, nextBlockIndex + paragraphs.Count() + 1);

                            var choices = ParseChoices(markdownDocument, nextBlock);

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

        private int SkipComments(MarkdownDocument markdownDocument, int index)
        {
            // All html blocks are considered comments, even if they are not `<!-- -->`
            while (index < markdownDocument.Count() && markdownDocument[index] is HtmlBlock)
            {
                index++;
            }

            return index;
        }

        private string CleanContent(IEnumerable<string> texts)
        {
            string dirty = string.Join("\n", texts);

            return CleanContent(dirty);

        }

        private string CleanContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Trim();
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

        private IEnumerable<string> ParseContent(MarkdownDocument markdownDocument, int index)
        {
            // Concat all paragraphs, skip the blocks already parsed.
            var paragraphs = markdownDocument.Skip(index).TakeWhile(x => x is ParagraphBlock).Select(GetBlockContent);

            return paragraphs.Any() ? paragraphs : Enumerable.Empty<string>();
        }

        private Dictionary<string, dynamic> ParseStats(MarkdownDocument markdownDocument, int index)
        {
            if (markdownDocument.Count() < index || !(markdownDocument[index] is ListBlock listBlock))
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

                var (key, value, _) = GetStat(content, true);

                stats.Add(key, value);
            }

            return stats;
        }

        private List<Choice> ParseChoices(MarkdownDocument markdownDocument, int index)
        {
            var choices = new List<Choice>();

            if (markdownDocument.Count() < index || !(markdownDocument[index] is ListBlock listBlock))
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

            string text = CleanContent(GetBlockContent(paragraphBlock));

            string sectionKey = paragraphBlock.Inline.Descendants<LinkInline>().FirstOrDefault().Url;

            var (conditions, effects, triggers) = ParseStatEffects(listItemBlock);

            return new Choice
            {
                Conditions = conditions,
                Effects = effects,
                SectionKey = sectionKey,
                Text = text,
                Triggers = triggers
            };
        }

        private (List<StatEffect> conditions, List<StatEffect> effects, List<StatEffect> triggers) ParseStatEffects(ListItemBlock listItemBlock)
        {
            var conditions = new List<StatEffect>();
            var effects = new List<StatEffect>();
            var triggers = new List<StatEffect>();

            // No effects, conditions, triggers.
            if (listItemBlock.Count < 2 || !(listItemBlock[1] is ListBlock listBlock))
            {
                return (conditions, effects, triggers);
            }

            foreach (ParagraphBlock paragraphBlock in listBlock.Descendants<ParagraphBlock>())
            {
                string content = GetBlockContent(paragraphBlock);

                (string key, dynamic value, EffectType effectType) = GetStat(content, false);

                var emphasis = paragraphBlock.Inline.FirstChild as EmphasisInline;

                if (emphasis != null && emphasis.IsDouble) // Is a Condition
                {
                    conditions.Add(new StatEffect
                    {
                        EffectType = effectType,
                        Key = key,
                        Value = value
                    });
                }
                else if (emphasis != null && !emphasis.IsDouble) // Is a Trigger
                {
                    triggers.Add(new StatEffect
                    {
                        EffectType = effectType,
                        Key = key,
                        Value = value
                    });
                }
                else // Is an effect
                {
                    effects.Add(new StatEffect
                    {
                        EffectType = effectType,
                        Key = key,
                        Value = value
                    });
                }
            }

            return (conditions, effects, triggers);
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

        private (string key, dynamic value, EffectType effectType) GetStat(string content, bool calculateValue)
        {
            if (TryGetCounterStat(content, calculateValue, out string key, out dynamic value, out EffectType effectType) || TryGetFlagStat(content, out key, out value, out effectType))
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

        private bool TryGetCounterStat(string content, bool calculateValue, out string key, out dynamic value, out EffectType effectType)
        {
            key = null;
            value = null;
            effectType = EffectType.None;

            var match = counterStatRegex.Match(content);

            if (match.Success)
            {
                if (calculateValue)
                {
                    value = RollParser.Roll(match.Groups[3].Value);
                }
                else
                {
                    value = match.Groups[3].Value;
                }

                key = match.Groups[1].Value;
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

        private string GetBlockContent<T>(MarkdownDocument markdownDocument, int index)
        {
            if (markdownDocument.Count() > index && markdownDocument[index] is T)
            {
                return GetBlockContent(markdownDocument[index]);
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
                // Parsing flags stats are separated by the parser on different indexes. Hence the need to join.
                return string.Join(string.Empty, paragraphBlock.Inline.Descendants<LiteralInline>().Select(x => x.Content.ToString()));
            }

            return null;
        }
    }
}
