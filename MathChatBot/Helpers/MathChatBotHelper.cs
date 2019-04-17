﻿using MathChatBot.Models;
using MathChatBot.Objects;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MathChatBot.Helpers
{

    public class MessageResult
    {
        public MessageResult()
        {
            IsSuccess = true;
        }

        public MessageResult(string errorMessage)
        {
            IsSuccess = false;
            Messages = new MessageObject[]{ new MessageObject()
            {
                MessageType = MessageTypes.BotMessage,
                Text = errorMessage,
                SendDate = DateTime.Now
            }};
        }

        public bool IsSuccess { get; set; }
        public MessageObject[] Messages { get; set; }
    }

    public class AnalyzeResult
    {
        public AnalyzeResult(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }

        public AnalyzeResult(List<string> searchStrings)
        {
            IsSuccess = true;
            SearchStrings = searchStrings;
        }

        public AnalyzeResult()
        {
            IsSuccess = true;
        }

        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> SearchStrings { get; set; }
        public bool IsCommand { get { return SearchStrings == null; } }
    }

    public class MathChatBotHelper : INotifyPropertyChanged
    {

        #region Properties

        private NLPHelper NLPHelper { get; set; }
        public SimpleCalculatorHelper SimpleCalculatorHelper { get; set; }
        private MathChatBotEntities MathChatBotEntities { get; set; }

        public List<MessageObject> LastBotMessagesAdded { get; set; }
        public MessageObject LastBotMessage { get; set; }
        public ObservableCollection<MessageObject> Messages { get; private set; }

        #endregion

        #region Interface

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Constructor

        public MathChatBotHelper()
        {
            NLPHelper = new NLPHelper();
            SimpleCalculatorHelper = new SimpleCalculatorHelper();
            MathChatBotEntities = DatabaseUtility.GetEntity();

            Messages = new ObservableCollection<MessageObject>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Write a welcome message to the user
        /// </summary>
        public void WriteWelcome()
        {
            WriteMessageToUser(Properties.Resources.welcome_to_mathchatbot);
        }

        /// <summary>
        /// Used for the seeing the terms for a given topic
        /// </summary>
        /// <param name="message">The message object for the topic</param>
        public void SeeTerms(MessageObject message)
        {
            if (!message.IsTopic)
            {
                WriteMessageToUser(Properties.Resources.please_select_a_topic_first_before_using_this_command);
                return;
            }

            var terms = message.Topic.Terms.Select(x => x.Name);
            var joined = string.Join("\n", terms);

            var text = string.Format(Properties.Resources.these_are_the_terms_under, message.Topic.Name.ToLower(), joined);

            WriteMessageToUser(text);
        }

        public void SeeExample(MessageObject message)
        {
            if (message == null || message.MessageType != MessageTypes.BotHelp)
            {
                WriteMessageToUser(Properties.Resources.please_select_a_term_first_before_using_this_command);
                return;
            }

            var examples = message.Material.MaterialExamples.OrderBy(x => x.ShowOrderId).ToList();

            if (examples.Count == 0)
            {
                WriteMessageToUser(Properties.Resources.this_term_has_no_examples);
                return;
            }

            var messages = new List<MessageObject>();

            messages.Add(
                new MessageObject()
                {
                    Text = string.Format(Properties.Resources.this_is_the_examples_i_found_for, message.Material.Term.Name.ToLower()),
                    MessageType = MessageTypes.BotMessage
                }
                );

            foreach (var example in examples)
            {
                messages.Add(new MessageObject(message.Material, example.ShowOrderId)
                {
                    MessageType = MessageTypes.BotHelp
                });
            }

            AddBotMessages(messages);
        }

        public void SeeDefinition(MessageObject message)
        {
            if (message.Material == null || !message.IsExample)
            {
                WriteMessageToUser(Properties.Resources.please_select_an_example_first_before_using_this_command);
                return;
            }

            string text = string.Format(Properties.Resources.this_is_what_i_found_about, message.Material.Term.Name.ToLower());

            AddBotMessages(new MessageObject[]{
                        new MessageObject()
                        {
                            Text = text,
                            MessageType = MessageTypes.BotMessage
                        },
                        new MessageObject()
                        {
                            Term = message.Material.Term,
                            Topic = message.Material.Topic,
                            MessageType = MessageTypes.BotHelp
                        }
                    }.ToList());
        }

        /// <summary>
        /// Write a message to the bot
        /// </summary>
        /// <param name="text">The message text</param>
        public void WriteMessageToBot(string text)
        {
            if (text == "")
                return;
            Messages.Add(new MessageObject()
            {
                MessageType = MessageTypes.User,
                Text = text,
                SendDate = DateTime.Now
            });

            var messageResult = AnalyzeText(text);
            if (messageResult.Messages != null)
                AddBotMessages(messageResult.Messages.ToList());
        }

        private void AddBotMessage(MessageObject message)
        {
            AddBotMessages(new MessageObject[] { message }.ToList());
        }

        private void AddBotMessages(List<MessageObject> messages)
        {
            LastBotMessagesAdded = messages;
            messages.ForEach(x => Messages.Add(x));
            LastBotMessage = messages.LastOrDefault();
        }

        /// <summary>
        /// Write a message to the user
        /// </summary>
        /// <param name="text">The message text</param>
        public void WriteMessageToUser(string text)
        {
            AddBotMessages(new MessageObject[] {
                new MessageObject()
                {
                    MessageType = MessageTypes.BotMessage,
                    Text = text
                }
            }.ToList());
        }

        /// <summary>
        /// Analyze a text message from the user
        /// </summary>
        /// <param name="text"></param>
        /// <returns>A message result</returns>
        private MessageResult AnalyzeText(string text)
        {
            var tempText = text.ToLower();

            // Get sentences that has been written
            var sentences = NLPHelper.GetSentences(tempText);

            // Does not support more than one sentence
            if (sentences.Count > 1)
                return new MessageResult(Properties.Resources.i_cannot_process_more_than_one_sentence);

            // Insert a question mark at the end of a text
            if (!tempText.EndsWith("?"))
                tempText += "?";

            // Tagging on the given words
            var wordList = new List<TaggedWord>();

            // If text has any numbers use the calculator
            if (SimpleCalculatorHelper.HasNumber(tempText))
            {
                // Replace natural language in the text with operators
                tempText = SimpleCalculatorHelper.ReplaceNaturalLanguage(tempText, removeSpaces: false);
                // Tag the words in the text
                wordList = NLPHelper.Tag(tempText);
                // Skip the WH-words and verbs in the beginning when using calculator
                var context = wordList.SkipWhile(x => x.IsWHWord || x.IsVerb).Where(x => x.POSStringIdentifier != ".").ToList();
                // Get text
                var joined = string.Join(string.Empty, context.Select(x => x.OriginalText));
                // Use calculator
                var output = SimpleCalculatorHelper.UseCalculator(joined);
                // If output is not null write the calculator result to the user
                if (output != null)
                {
                    return new MessageResult()
                    {
                        Messages = new MessageObject[]{
                        new MessageObject(output)
                    }
                    };
                }
            }

            // Tag the words in the text
            wordList = NLPHelper.Tag(tempText);

            // Analyze the input
            var analyzeResult = AnalyzeWordList(wordList);

            // If success
            if (analyzeResult.IsSuccess)
            {
                // If command has been executed return empty result
                if (analyzeResult.IsCommand)
                    return new MessageResult();

                // Analyze the search strings
                return AnalyzeSearchStrings(analyzeResult.SearchStrings);
            }
            // If error
            else
                return new MessageResult(analyzeResult.ErrorMessage);
        }

        /// <summary>
        /// Analyze the wordlist to generate a search string list
        /// </summary>
        /// <param name="wordList"></param>
        /// <returns></returns>
        private AnalyzeResult AnalyzeWordList(List<TaggedWord> wordList)
        {
            var stringList = new List<string>();

            var onlyWordsList = wordList.Where(x => x.POSIdentifier != TaggedWord.POSTag.NONE).ToList();
            var onlyNounsList = wordList.Where(x => x.IsNoun).ToList();

            // Check if there has been given a command
            string joined = string.Join(string.Empty, onlyWordsList.Select(x => x.OriginalText));
            if (IsCommand(joined))
                return new AnalyzeResult((new string[] { joined }).ToList());

            // No words or nouns entered by the user
            if (onlyWordsList.Count == 0 || onlyNounsList.Count == 0)
                return new AnalyzeResult(Properties.Resources.i_did_not_understand_that_sentence);

            var hashSet = new HashSet<string>();

            // Index of the first noun or adjective in the phrase
            var firstIndex = wordList.FindIndex(x => x.IsNoun || x.IsAdjective);
            // Index of the last noun or adjective in the phrase
            var lastIndex = wordList.FindLastIndex(x => x.IsNoun || x.IsAdjective);
            // Get all nouns, adjectives and words inbetween
            var range = wordList.GetRange(firstIndex, lastIndex - firstIndex + 1).ToList();

            for (int i = 0; i < range.Count; i++)
            {
                var word = range[i];

                if (word.IsAdjective)
                {
                    var nextIndex = i + 1;
                    // Check for erros in sentence
                    if (nextIndex == range.Count || (!range[nextIndex].IsNoun && !range[nextIndex].IsAdjective))
                        return new AnalyzeResult(Properties.Resources.please_write_a_proper_sentence);
                }
            }

            // Get indexes of all the commas
            var indexes = range.Select(x =>
            {
                return x.Word == "," ? range.IndexOf(x) : -1;
            })
            .Where(x => x != -1)
            .ToList();

            // Check if any commas are located beside each other
            for (int i = 0; i < indexes.Count - 1; i++)
            {
                if (indexes[i + 1] - indexes[i] == 1)
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
            }

            var notNounsAndAdjectives = range.Where(x => !x.IsAdjective && !x.IsNoun).ToList();

            if (notNounsAndAdjectives.Any(x => x.Word == "," || x.Word == "and"))
            {
                if (notNounsAndAdjectives.Last().Word != "and")
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
                else if (notNounsAndAdjectives.Count > 1 && notNounsAndAdjectives.SkipWhile(x => x.Word == ",").ToList().Count != 1)
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
            }

            // Join words
            var nounCollection = string.Join("", range.Select(x => x.OriginalText));
            // Remove white space before comma
            nounCollection = nounCollection.Replace(" ,", ",");
            // Add noun collection search string
            hashSet.Add(nounCollection.ToLower());

            var stringWordList = range.Select(x =>
            {
                if (!x.IsAdjective && !x.IsNoun)
                {
                    x.POSIdentifier = TaggedWord.POSTag.NONE;
                    x.Word = "|";
                }
                return x.Word;
            }).ToList();

            var joinedWords = string.Join(" ", stringWordList);
            var splitted = joinedWords.Split('|').ToList();
            splitted = splitted.Select(x => { x = x.Trim(); return x; }).Where(x => x != "").ToList();

            splitted.ForEach(x => hashSet.Add(x));
            stringList = hashSet.ToList();

            return new AnalyzeResult(stringList);
        }

        /// <summary>
        /// Analyze a given list of search strings
        /// </summary>
        /// <param name="list">The list with the search strings</param>
        /// <returns>A message result</returns>
        private MessageResult AnalyzeSearchStrings(List<string> list)
        {
            foreach (var str in list)
            {
                // Checking for bot command
                if (RunCommand(str))
                    return new MessageResult();

                Topic topic = null;
                Term term = MathChatBotEntities.Terms.FirstOrDefault(x => x.Name.ToLower() == str);

                if (term == null)
                {
                    topic = MathChatBotEntities.Topics.FirstOrDefault(x => x.Name.ToLower() == str);
                    if (topic == null)
                        continue;
                }

                string message = string.Format(Properties.Resources.this_is_what_i_found_about, term != null ? term.Name.ToLower() : topic.Name.ToLower());

                return new MessageResult()
                {
                    Messages = new MessageObject[]{
                        new MessageObject()
                        {
                            Text = message,
                            MessageType = MessageTypes.BotMessage
                        },
                        new MessageObject()
                        {
                            Term = term,
                            Topic = topic,
                            MessageType = MessageTypes.BotHelp
                        }
                    }
                };
            }

            // One noun
            if (list.Count == 1)
                return new MessageResult(string.Format(Properties.Resources.i_do_not_know_anything_about_that_noun, list[0], GetTopics()));
            // More nouns
            else
                return new MessageResult(string.Format(Properties.Resources.i_do_not_know_anything_about_those_nouns, GetTopics()));

        }

        private bool IsCommand(string text)
        {
            var commands = new List<string>();
            commands.Add(Properties.Resources.see_term);
            commands.Add(Properties.Resources.see_terms);
            commands.Add(Properties.Resources.see_example);
            commands.Add(Properties.Resources.see_examples);
            commands.Add(Properties.Resources.see_definition);
            commands.Add(Properties.Resources.clear);
            commands.Add(Properties.Resources.topic);
            commands.Add(Properties.Resources.topics);
            commands.Add(Properties.Resources.term);
            commands.Add(Properties.Resources.terms);
            commands.Add(Properties.Resources.help);
            commands.Add(Properties.Resources.hello);
            commands.Add(Properties.Resources.what_is_your_name);
            commands.Add(Properties.Resources.what_is_the_meaning_of_life);
            commands.Add(Properties.Resources.tell_me_a_joke);
            commands.Add(Properties.Resources.who_is_your_creator);
            commands.Add(Properties.Resources.does_god_exist);
            commands.Add(Properties.Resources.any_news);
            commands = commands.Select(x => x.ToLower()).ToList();
            return commands.Any(x => x == text);
        }

        /// <summary>
        /// Check if there has been given any commands
        /// </summary>
        /// <param name="text">The message text</param>
        /// <returns>A string output for the given command</returns>
        private bool RunCommand(string text)
        {
            text = text.ToLower();

            // See term 
            // See terms
            if (
              Properties.Resources.see_term.ToLower() == text ||
              Properties.Resources.see_terms.ToLower() == text
              )
            {
                SeeTerms(LastBotMessage);
                return true;
            }
            // See example 
            // See examples
            if (
              Properties.Resources.see_example.ToLower() == text ||
              Properties.Resources.see_examples.ToLower() == text
              )
            {
                SeeExample(LastBotMessage);
                return true;
            }
            // See definition
            else if (Properties.Resources.see_definition.ToLower() == text)
            {
                SeeDefinition(LastBotMessage);
                return true;
            }
            // See definition
            else if (Properties.Resources.see_definition.ToLower() == text)
            {
                SeeDefinition(LastBotMessage);
                return true;
            }
            // Clear
            else if (Properties.Resources.clear.ToLower() == text)
            {
                Messages.Clear();
                WriteWelcome();
                return true;
            }
            // Topic
            // Topics
            else if (
              Properties.Resources.topic.ToLower() == text ||
              Properties.Resources.topics.ToLower() == text
              )
            {
                AddBotMessage(new MessageObject(string.Format(Properties.Resources.these_are_the_topics_i_know_about, GetTopics())));
                return true;
            }
            // Term
            // Terms
            else if (
              Properties.Resources.term.ToLower() == text ||
              Properties.Resources.terms.ToLower() == text
              )
            {
                AddBotMessage(new MessageObject(string.Format(Properties.Resources.please_select_one_of_these_topics_first, GetTopics())));
                return true;
            }
            // Help
            else if (Properties.Resources.help.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.help_message));
                return true;
            }
            // Hello
            else if (Properties.Resources.hello.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.hello_response));
                return true;
            }
            // What is your name
            else if (Properties.Resources.what_is_your_name.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.what_is_your_name_response));
                return true;
            }
            // What is the meaning of life
            else if (Properties.Resources.what_is_the_meaning_of_life.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.what_is_the_meaning_of_life_response));
                return true;
            }
            // Tell me a joke
            else if (Properties.Resources.tell_me_a_joke.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.tell_me_a_joke_response));
                return true;
            }
            // Who is your creator
            else if (Properties.Resources.who_is_your_creator.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.who_is_your_creator_response));
                return true;
            }
            // Does god exist
            else if (Properties.Resources.does_god_exist.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.does_god_exist_response));
                return true;
            }
            // Any news
            else if (Properties.Resources.any_news.ToLower() == text)
            {
                AddBotMessage(new MessageObject(Properties.Resources.any_news_response));
                return true;
            }

            return false;
        }

        /// <summary>
        /// // Get a list with topics saved in the database
        /// </summary>
        /// <returns>A string presentation of the topics</returns>
        private string GetTopics()
        {
            var topics = MathChatBotEntities.Topics.Select(x => x.Name).ToList();
            return string.Join("\n", topics);
        }

        #endregion

    }
}