using MathChatBot.Models;
using MathChatBot.Objects;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathChatBot.Helpers
{

    /// <summary>
    /// Result returned after a message has been generated as response
    /// </summary>
    public class MessageResult
    {
        public MessageResult()
        {
            IsSuccess = true;
        }

        public MessageResult(string errorMessage)
        {
            IsSuccess = false;
            Messages = new MessageObject[] { new MessageObject(errorMessage) };
        }

        public MessageResult(MessageObject[] messageObjects)
        {
            IsSuccess = true;
            Messages = messageObjects;
        }

        public bool IsSuccess { get; set; }
        public MessageObject[] Messages { get; set; }
    }

    /// <summary>
    /// Anylyze result used for when analyzing the words in the given message text
    /// </summary>
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

    public class ChatBotCommand
    {
        public string[] CommandTexts { get; set; }
        public Action Action { get; set; }
        public Func<string> Func { get; set; }

        public ChatBotCommand(string[] commandTexts, Action action)
        {
            CommandTexts = commandTexts;
            Action = action;
        }

        public ChatBotCommand(string[] commandTexts, Func<string> func)
        {
            CommandTexts = commandTexts;
            Func = func;
        }

        public void RunAction()
        {
            Action?.Invoke();
        }

        public string RunFunc()
        {
            return Func?.Invoke();
        }
    }

    /// <summary>
    /// Interaction logic for the MathChatBotHelper
    /// </summary>
    public class MathChatBotHelper
    {

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        public User User { get; set; }
        private NLPHelper NLPHelper { get; set; }
        public SimpleCalculatorHelper SimpleCalculatorHelper { get; set; }
        private MathChatBotEntities Entity { get { return DatabaseUtility.Entity; } }
        public List<MessageObject> LastBotMessagesAdded { get; set; }
        public MessageObject LastBotMessage { get; set; }
        public ObservableCollection<MessageObject> Messages { get; private set; }

        public Assignment CurrentAssignment { get; private set; }
        public List<Assignment> SelectedAssignments { get; private set; }

        public List<ChatBotCommand> Commands { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public MathChatBotHelper()
        {
            NLPHelper = new NLPHelper();
            SimpleCalculatorHelper = new SimpleCalculatorHelper();
            Messages = new ObservableCollection<MessageObject>();

            GetCommands();
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Get commands for the chatbot
        /// </summary>
        public void GetCommands()
        {
            Commands = new List<ChatBotCommand>();

            Commands.Add(new ChatBotCommand(
                new string[] {
                    Properties.Resources.see_term.ToLower(),
                    Properties.Resources.see_terms.ToLower()
                },
                () => SeeTerms(LastBotMessage)
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.see_example.ToLower(),
                    Properties.Resources.see_examples.ToLower()
                },
                () => SeeExample(LastBotMessage)
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.see_definition.ToLower()
                },
                () => SeeDefinition(LastBotMessage)
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.clear.ToLower()
                },
                () =>
                {
                    Messages.Clear();
                    WriteWelcome();
                }));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.topic.ToLower(),
                    Properties.Resources.topics.ToLower()
                },
                () =>
                {
                    if (LastBotMessage != null && LastBotMessage.IsSelection)
                        SeeDefinitions(LastBotMessage, true);
                    else
                        AddBotMessage(new MessageObject(string.Format(Properties.Resources.these_are_the_topics_i_know_about, GetTopics())));
                }
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.term.ToLower(),
                    Properties.Resources.terms.ToLower()
                },
                () =>
                {
                    if (LastBotMessage != null && LastBotMessage.IsSelection)
                        SeeDefinitions(LastBotMessage, false);
                    else
                        AddBotMessage(new MessageObject(string.Format(Properties.Resources.please_select_one_of_these_topics_first, GetTopics())));
                }
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.help.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.help_message))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.hello.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.hello_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.what_is_your_name.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.what_is_your_name_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.what_is_the_meaning_of_life.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.what_is_the_meaning_of_life_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.tell_me_a_joke.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.tell_me_a_joke_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.who_is_your_creator.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.who_is_your_creator_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.any_news.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.any_news_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.does_god_exist.ToLower()
                },
                () => AddBotMessage(new MessageObject(Properties.Resources.does_god_exist_response))
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.current.ToLower()
                },
                () => SeeCurrentAssignment()
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.next.ToLower()
                },
                () => SeeNextAssignment()
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.previous.ToLower()
                },
                () => SeePreviousAssignment()
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.did_not_help_command.ToLower(),
                    Properties.Resources.need_help_command.ToLower()
                },
                () => DidNotHelp(LastBotMessage)
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.see_assignments.ToLower()
                },
                () => SeeAssignments(LastBotMessage)
                ));
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.see_answers.ToLower()
                },
                () => SeeAnswers(LastBotMessage)
                ));
        }

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
            if (!message.IsTopicDefinition)
            {
                WriteMessageToUser(Properties.Resources.please_select_a_topic_first_before_using_this_command);
                return;
            }

            var terms = message.Topic.Terms.Select(x => x.Name);
            var joined = string.Join("\n", terms);

            var text = string.Format(Properties.Resources.these_are_the_terms_under, message.Topic.Name.ToLower(), joined);

            WriteMessageToUser(text);
        }

        /// <summary>
        /// See example for a term
        /// </summary>
        /// <param name="message">The message object</param>
        public void SeeExample(MessageObject message)
        {
            if (message == null || !message.IsMaterial)
            {
                WriteMessageToUser(Properties.Resources.please_select_a_term_first_before_using_this_command);
                return;
            }

            var examples = message.Material.MaterialExamples
                .OrderBy(x => x.ShowOrderId)
                .ToList();

            if (!examples.Any())
            {
                WriteMessageToUser(Properties.Resources.this_term_has_no_examples);
                return;
            }

            var messages = new List<MessageObject>();

            // Bot message
            messages.Add(new MessageObject(string.Format(Properties.Resources.this_is_the_examples_i_found_for, message.Material.Term.Name.ToLower())));
            // Examples
            foreach (var example in examples)
                messages.Add(new MessageObject(example));

            AddBotMessages(messages);
        }

        /// <summary>
        /// See definition for an example
        /// </summary>
        /// <param name="message">The message object</param>
        public void SeeDefinition(MessageObject message)
        {
            if (!message.IsExample)
            {
                WriteMessageToUser(Properties.Resources.please_select_an_example_first_before_using_this_command);
                return;
            }

            string text = string.Format(Properties.Resources.this_is_what_i_found_about, message.Material.Term.Name.ToLower());

            AddBotMessages(new MessageObject[]
            {
                new MessageObject(text),
                new MessageObject(message.Material)
            }.ToList());
        }

        public void SeeDefinitions(MessageObject message, bool selectedTopic)
        {
            if (!message.IsSelection)
            {
                WriteMessageToUser(Properties.Resources.you_can_only_use_this_command_when_having_a_selection);
                return;
            }

            var messages = new List<MessageObject>();

            string text = string.Format(Properties.Resources.this_is_what_i_found_about, selectedTopic ? message.Topic.Name : message.Term.Name);
            // Add bot message
            messages.Add(new MessageObject(text));

            if (!selectedTopic)
            {
                // Add bot help
                foreach (var material in message.Term.Materials)
                    messages.Add(new MessageObject(material));
            }
            else
            {
                // Add bot help
                foreach (var material in message.Topic.Materials)
                    messages.Add(new MessageObject(material));
            }

            AddBotMessages(messages);
        }

        /// <summary>
        /// Did not help command for terms and examples
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="message">The message object</param>
        public void DidNotHelp(MessageObject message)
        {
            var userRoles = DatabaseUtility.GetUserRoles(User.Username);

            // If you are not a student you cannot make help requests
            if (!userRoles.Any(x => x == Role.RoleTypes.Student))
            {
                WriteMessageToUser(Properties.Resources.you_need_to_be_a_student_to_make_help_requests);
                return;
            }

            // Create help request
            var helpRequest = new HelpRequest()
            {
                User = User,
                Material = !message.IsMaterial ? null : message.Material,
                MaterialExample = !message.IsExample ? null : message.MaterialExample,
                Assignment = !message.IsAssignment ? null : message.Assignment
            };

            int termId = helpRequest.Material != null ? helpRequest.Material.TermId.Value : 0;
            if (termId == 0)
                termId = helpRequest.MaterialExample != null ? helpRequest.MaterialExample.Material.TermId.Value : 0;
            if (termId == 0)
                termId = helpRequest.Assignment != null ? helpRequest.Assignment.TermId : 0;

            if (termId == 0)
            {
                WriteMessageToUser(Properties.Resources.could_not_make_a_help_request);
                return;
            }

            helpRequest.TermId = termId;

            // You cannot use null propagation in linq so therefore there is used temp values
            var materialId = helpRequest.Material?.Id;
            var materialExampleId = helpRequest.MaterialExample?.Id;
            var assignmentId = helpRequest.Assignment?.Id;

            if (!Entity.HelpRequests.Any(
                x => x.UserId == helpRequest.User.Id &&
                x.MaterialId == materialId &&
                x.MaterialExampleId == materialExampleId &&
                x.AssignmentId == assignmentId)
                )
            {
                Entity.HelpRequests.Add(helpRequest);
                if (Entity.SaveChanges() == 1)
                    WriteMessageToUser(Properties.Resources.help_request_has_been_sent_to_your_teacher);
                else
                    WriteMessageToUser(Properties.Resources.could_not_make_a_help_request);

                return;
            }

            WriteMessageToUser(Properties.Resources.you_have_already_sent_a_help_request_for_this_material);
        }

        /// <summary>
        /// See current assignment
        /// </summary>
        public void SeeCurrentAssignment()
        {
            if (CurrentAssignment == null)
            {
                WriteMessageToUser(Properties.Resources.you_have_not_started_any_assignments_yet);
                return;
            }

            AddBotMessage(new MessageObject(CurrentAssignment));
        }

        /// <summary>
        /// See next assignment
        /// </summary>
        public void SeeNextAssignment()
        {
            if (CurrentAssignment == null)
            {
                WriteMessageToUser(Properties.Resources.you_have_not_started_any_assignments_yet);
                return;
            }

            if (SelectedAssignments != null && SelectedAssignments.Count > 1 && SelectedAssignments.IndexOf(CurrentAssignment) != SelectedAssignments.Count - 1)
            {
                CurrentAssignment = SelectedAssignments[SelectedAssignments.IndexOf(CurrentAssignment) + 1];
                SeeCurrentAssignment();
            }
            else
                WriteMessageToUser(Properties.Resources.there_are_no_more_assignments);
        }

        /// <summary>
        /// See previous assignment
        /// </summary>
        public void SeePreviousAssignment()
        {
            if (CurrentAssignment == null)
            {
                WriteMessageToUser(Properties.Resources.you_have_not_started_any_assignments_yet);
                return;
            }

            if (SelectedAssignments.IndexOf(CurrentAssignment) != 0)
            {
                CurrentAssignment = SelectedAssignments[SelectedAssignments.IndexOf(CurrentAssignment) - 1];
                SeeCurrentAssignment();
            }
            else
                WriteMessageToUser(Properties.Resources.there_are_no_previous_assignments);
        }

        /// <summary>
        /// See assignments for an topic, term or example
        /// </summary>
        /// <param name="message">The message object</param>
        public void SeeAssignments(MessageObject message)
        {
            List<Assignment> assignments = null;
            string text = null;

            if (message.IsTermDefinition || message.IsExample)
            {
                assignments = message.Material.Term.Assignments
                .OrderBy(x => x.AssignmentNo)
                .ToList();

                if (assignments.Any())
                    text = string.Format(Properties.Resources.there_are_for_this_term_this_is_the_first_assignment, assignments.Count);
            }
            else if (message.IsTopicDefinition)
            {
                assignments = new List<Assignment>();
                message.Topic.Terms.OrderBy(x => x.Name)
                    .Select(x =>
                    {
                        var a = x.Assignments.OrderBy(x2 => x2.AssignmentNo);
                        assignments.AddRange(a);
                        return x;
                    })
                    .ToList();

                if (assignments.Any())
                    text = string.Format(Properties.Resources.there_are_under_this_topic_this_is_the_first_assignment, assignments.Count);
            }
            else
            {
                WriteMessageToUser(Properties.Resources.please_select_a_term_or_example_first_before_using_this_command);
                return;
            }

            if (assignments.Any())
            {
                SelectedAssignments = assignments;
                CurrentAssignment = SelectedAssignments.FirstOrDefault();

                AddBotMessages(new MessageObject[]{
                        new MessageObject(text),
                        new MessageObject(CurrentAssignment)
                    }.ToList());
            }
            else
                WriteMessageToUser(Properties.Resources.there_are_no_assignments_for_this);
        }

        /// <summary>
        /// See answers for an assignment
        /// </summary>
        /// <param name="message">The message object</param>
        public void SeeAnswers(MessageObject message)
        {
            // If the message object is not an assignment
            if (!message.IsAssignment)
            {
                WriteMessageToUser(Properties.Resources.please_select_an_assignment_first_before_using_this_command);
                return;
            }

            // Get all members from assignment object that starts with "Answer" 
            var members = typeof(Assignment).GetMembers()
                .Where(x => x.Name.StartsWith("Answer") && !x.Name.Contains("_"))
                .ToList();

            var text = string.Empty;

            // Get all answers for the assignment
            foreach (var member in members)
            {
                var value = message.Assignment.GetPropertyValue(member.Name);
                if (value != null)
                {
                    var assignmentLetter = member.Name.ReplaceIgnoreCase("answer", "").ToLower();
                    var strValue = value.ToString();

                    text += string.Format("{0}{1}) {2}", text == string.Empty ? string.Empty : "\n", assignmentLetter, strValue);
                }
            }

            // Check if there are any answers
            if (text != string.Empty)
                AddBotMessage(new MessageObject(string.Format(Properties.Resources.this_is_the_answers_for_the_assignment, text)));
            else
                WriteMessageToUser(Properties.Resources.this_assignment_has_no_answers);
        }

        /// <summary>
        /// Write a message to the bot
        /// </summary>
        /// <param name="text">The message text</param>
        public void WriteMessageToBot(string text)
        {
            if (text == "")
                return;

            Messages.Add(new MessageObject(MessageTypes.User, text));

            var messageResult = AnalyzeText(text);
            if (messageResult.Messages != null)
                AddBotMessages(messageResult.Messages.ToList());
        }

        /// <summary>
        /// Add a single bot message
        /// </summary>
        /// <param name="message">The message object</param>
        private void AddBotMessage(MessageObject message)
        {
            AddBotMessages(new MessageObject[] { message }.ToList());
        }

        /// <summary>
        /// Add multiple bot messages
        /// </summary>
        /// <param name="messages">The list of message objects</param>
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
            AddBotMessage(new MessageObject(text));
        }

        /// <summary>
        /// Analyze a text message from the user
        /// </summary>
        /// <param name="text"></param>
        /// <returns>A message result</returns>
        private MessageResult AnalyzeText(string text)
        {
            var tempText = text.ToLower();

            PerformanceTester.StartMET("GetSentences");
            // Get sentences that has been written
            var sentences = NLPHelper.GetSentences(tempText);
            PerformanceTester.StopMET("GetSentences");

            // Does not support more than one sentence
            if (sentences.Count > 1)
                return new MessageResult(Properties.Resources.i_cannot_process_more_than_one_sentence);

            // Tagging on the given words
            List<TaggedWord> wordList = null;

            // Function for tagging
            Action tagging = () =>
            {
                PerformanceTester.StartMET("Tagging");
                // Insert a question mark at the end of a text
                if (!tempText.EndsWith("?"))
                    tempText += "?";
                // Tag the words in the text
                wordList = NLPHelper.Tag(tempText);
                PerformanceTester.StopMET("Tagging");
            };

            PerformanceTester.StartMET("Use calculator");
            // If text has any numbers use the calculator
            string output;
            if (SimpleCalculatorHelper.CheckInput(ref tempText, out output))
            {
                // If text is not math command
                if (output == null)
                {
                    var input = tempText;

                    tagging();

                    if (wordList.Any(x => x.IsWHWord || x.IsVerb))
                    {
                        // Skip the WH-words and verbs in the beginning when using calculator
                        var context = wordList
                            .SkipWhile(x => x.IsWHWord || x.IsVerb)
                            .Where(x => x.POSStringIdentifier != ".")
                            .ToList();
                        // Get text
                        var joined = string.Join(string.Empty, context.Select(x => x.OriginalText));
                        // If sentence starts with a WH word insert an equal sign
                        if (wordList.FirstOrDefault().IsWHWord)
                            input = joined.Insert(0, "=");
                    }

                    // Use calculator
                    output = SimpleCalculatorHelper.UseCalculator(input);
                    PerformanceTester.StopMET("Use calculator");
                }

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

            tagging();

            PerformanceTester.StartMET("Analyze word list");
            // Analyze the input
            var analyzeResult = AnalyzeWordList(wordList);
            PerformanceTester.StopMET("Analyze word list");

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
        /// <param name="wordList">The list with the tagged words</param>
        /// <returns></returns>
        private AnalyzeResult AnalyzeWordList(List<TaggedWord> wordList)
        {
            var stringList = new List<string>();

            // Get only the words
            var onlyWordsList = wordList.Where(x => x.POSIdentifier != TaggedWord.POSTag.NONE).ToList();
            // Get only nouns
            var onlyNounsList = wordList.Where(x => x.IsNoun).ToList();

            // Check if there has been given a command
            string joined = string.Join(string.Empty, onlyWordsList.Select(x => x.OriginalText));
            if (IsCommand(joined))
                return new AnalyzeResult((new string[] { joined }).ToList());

            // No words or nouns entered by the user
            if (onlyWordsList.Count == 0 || onlyNounsList.Count == 0)
                return new AnalyzeResult(Properties.Resources.i_did_not_understand_that_sentence);

            // Using hashset to avoid having search string dublets
            var hashSet = new HashSet<string>();

            // Index of the first noun or adjective in the phrase
            var firstIndex = wordList.FindIndex(x => x.IsNoun || x.IsAdjective);
            // Index of the last noun or adjective in the phrase
            var lastIndex = wordList.FindLastIndex(x => x.IsNoun || x.IsAdjective);
            // Get all nouns, adjectives and words inbetween
            var range = wordList.GetRange(firstIndex, lastIndex - firstIndex + 1).ToList();

            if (range.Any(x => x.IsAdjective))
            {
                // Checking range
                for (int i = 0; i < range.Count; i++)
                {
                    var word = range[i];

                    if (word.IsAdjective)
                    {
                        var nextIndex = i + 1;
                        // If not a noun or an adjective follows an adjective the sentence is not proper
                        if (nextIndex == range.Count || (!range[nextIndex].IsNoun && !range[nextIndex].IsAdjective))
                            return new AnalyzeResult(Properties.Resources.please_write_a_proper_sentence);
                    }
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
                // If the index difference is 1 then they are written beside each other
                if (indexes[i + 1] - indexes[i] == 1)
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
            }

            // Check for a proper list representation of nouns
            var notNounsAndAdjectives = range.Where(x => !x.IsAdjective && !x.IsNoun).ToList();
            if (notNounsAndAdjectives.Any(x => x.Word == "," || x.Word == Properties.Resources.and))
            {
                // Error if the last word is not "and"
                if (notNounsAndAdjectives.Last().Word != Properties.Resources.and)
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
                // Error if there is commas but it ends with more than just one "and" like (dog, bird and bee and tiger)
                else if (notNounsAndAdjectives.Count > 1 && notNounsAndAdjectives.SkipWhile(x => x.Word == ",").ToList().Count != 1)
                    return new AnalyzeResult(Properties.Resources.please_write_a_proper_list_representation_of_nouns);
            }

            // Join words
            var nounCollection = string.Join("", range.Select(x => x.OriginalText));
            // Remove white space before comma
            nounCollection = nounCollection.Replace(" ,", ",");
            // Add noun collection search string
            hashSet.Add(nounCollection.ToLower());

            // Get remaining nouns and adjective collections
            var tempRange = range;
            do
            {
                // First nouns and adjectives in range
                var collection = tempRange.TakeWhile(x => x.IsNoun || x.IsAdjective).ToList();
                // Join them
                var join = string.Join(string.Empty, collection.Select(x => x.OriginalText)).Trim();
                // Add them list hashset
                hashSet.Add(join);
                // If the collection consists of only nouns and there 
                // are several then add each of them to the hashset
                if (collection.Count > 1 && collection.All(x => x.IsNoun))
                    collection.ForEach(x => hashSet.Add(x.Word));
                // Skip them plus the seperator
                tempRange = tempRange.Skip(collection.Count + 1).ToList();
            } while (tempRange.Any());

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
            PerformanceTester.StartMET("Analyze search strings");

            foreach (var str in list)
            {
                // Checking for bot command
                if (RunCommand(str))
                    return new MessageResult();

                Term term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == str);
                Topic topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == str);
                if (topic == null && term == null)
                    continue;

                List<MessageObject> messages = new List<MessageObject>();
                List<Material> materials = null;

                // A term and a topic has the same name
                if (term != null && topic != null)
                    messages.Add(new MessageObject(term, topic));
                // Get materials for the topic
                else if (term == null)
                {
                    materials = Entity.Materials
                        .Where(x => x.TopicId == topic.Id)
                        .OrderBy(x => x.ShowOrderId)
                        .ToList();
                }
                // Get materials for the term
                else
                {
                    materials = Entity.Materials
                        .Where(x => x.TermId == term.Id)
                        .OrderBy(x => x.ShowOrderId)
                        .ToList();
                }

                if (materials != null)
                {
                    // Add bot message
                    messages.Add(new MessageObject(string.Format(Properties.Resources.this_is_what_i_found_about, term != null ? term.Name.ToLower() : topic.Name.ToLower())));
                    // Add materials            
                    foreach (var material in materials)
                        messages.Add(new MessageObject(material));
                }

                PerformanceTester.StopMET("Analyze search strings");

                return new MessageResult(messages.ToArray());
            }

            PerformanceTester.StopMET("Analyze search strings");

            // One noun
            if (list.Count == 1)
                return new MessageResult(string.Format(Properties.Resources.i_do_not_know_anything_about_that_noun, list[0], GetTopics()));
            // More nouns
            else
                return new MessageResult(string.Format(Properties.Resources.i_do_not_know_anything_about_those_nouns, GetTopics()));

        }

        /// <summary>
        /// Check if the text is a command
        /// </summary>
        /// <param name="text">The message text</param>
        /// <returns></returns>
        private bool IsCommand(string text)
        {
            return Commands.Any(x => x.CommandTexts.Contains(text));
        }

        /// <summary>
        /// Check if there has been given any commands
        /// </summary>
        /// <param name="text">The message text</param>
        /// <returns>A string output for the given command</returns>
        public bool RunCommand(string text)
        {
            text = text.ToLower().Replace("?", "");
            var command = Commands.FirstOrDefault(x => x.CommandTexts.Contains(text));

            if (command == null)
                return false;

            command.RunAction();
            return true;
        }

        /// <summary>
        /// // Get a list with topics saved in the database
        /// </summary>
        /// <returns>A string presentation of the topics</returns>
        private string GetTopics()
        {
            var topics = DatabaseUtility.GetTopicNames();
            return string.Join("\n", topics);
        }

        #endregion

    }

}
