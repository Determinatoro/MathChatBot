using MathChatBot.Models;
using MathChatBot.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MathChatBot.Objects
{
    public enum MessageTypes
    {
        BotHelp,
        BotMessage,
        BotSelection,
        User
    }

    public class MessageObject
    {
        private Term _term;
        private Topic _topic;

        public MessageObject(string text)
        {
            Text = text;
            MessageType = MessageTypes.BotMessage;
        }

        public MessageObject(MessageTypes messageTypes, string text)
        {
            Text = text;
            MessageType = messageTypes;
        }

        public MessageObject(MaterialExample materialExample)
        {
            Material = materialExample.Material;
            MaterialExample = materialExample;
            Source = MaterialExample.Source;
            MessageType = MessageTypes.BotHelp;
        }

        public MessageObject(Assignment assignment)
        {
            Assignment = assignment;
            Source = Assignment.Source;
            MessageType = MessageTypes.BotHelp;
        }

        public MessageObject(Material material)
        {
            Material = material;
            Source = Material.Source;
            MessageType = MessageTypes.BotHelp;
        }

        public MessageObject(Term term, Topic topic)
        {
            _term = term;
            _topic = topic;
            Text = Properties.Resources.what_you_searched_for_is_both_a_topic_and_a_term_which_one_do_you_want;
            MessageType = MessageTypes.BotSelection;
        }

        public bool IsExample
        {
            get { return MaterialExample != null; }
        }
        public bool IsTopicDefinition
        {
            get
            {
                return Topic != null && IsMaterial;
            }
        }
        public bool IsTermDefinition
        {
            get
            {
                return Term != null && IsMaterial;
            }
        }
        public bool IsAssignment
        {
            get
            {
                return Assignment != null;
            }
        }
        public bool IsMaterial
        {
            get
            {
                return Material != null && !IsExample;
            }
        }
        public bool IsSelection
        {
            get { return _term != null && _topic != null; }
        }

        public string Source { get; private set; }
        public string Text { get; set; }
        public MessageTypes MessageType { get; private set; }

        public MaterialExample MaterialExample { get; private set; }
        public Material Material { get; private set; }
        public Assignment Assignment { get; private set; }
        public Term Term
        {
            get
            {
                if (_term != null)
                    return _term;
                else if (IsMaterial)
                    return Material.Term;
                else if (IsAssignment)
                    return Assignment.Term;
                else if (IsExample)
                    return Material.Term;

                return null;
            }
        }
        public Topic Topic
        {
            get
            {
                if (_topic != null)
                    return _topic;
                else if (IsMaterial)
                    return Material.Topic;

                return null;
            }
        }

        #region UI specific

        public bool ShowExampleButton
        {
            get
            {
                return IsMaterial && Material.MaterialExamples.Any();
            }
        }
        public Brush MessageBackground
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.User:
                        {
                            return Application.Current.Resources["UserMessageColor"] as SolidColorBrush;
                        }
                    case MessageTypes.BotHelp:
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                        {
                            return Application.Current.Resources["BotMessageColor"] as SolidColorBrush;
                        }
                }

                return new SolidColorBrush(Colors.Gray);
            }
        }
        public Brush MessageForeground
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.User:
                        {
                            return Application.Current.Resources["UserMessageTextColor"] as SolidColorBrush;
                        }
                    case MessageTypes.BotHelp:
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                        {
                            return Application.Current.Resources["BotMessageTextColor"] as SolidColorBrush;
                        }
                }

                return new SolidColorBrush(Colors.Black);
            }
        }
        public BitmapImage Image
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                    case MessageTypes.User:
                        {
                            return null;
                        }
                    case MessageTypes.BotHelp:
                        {
                            return Source == null ? null : Utility.Base64ToImage(Source);
                        }
                }

                return null;
            }
        }
        public Visibility MessageVisibility
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                    case MessageTypes.User:
                        {
                            return Visibility.Visible;
                        }
                    case MessageTypes.BotHelp:
                        {
                            return Visibility.Collapsed;
                        }
                }

                return Visibility.Collapsed;
            }
        }
        public Visibility ImageVisibility
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                    case MessageTypes.User:
                        {
                            return Visibility.Collapsed;
                        }
                    case MessageTypes.BotHelp:
                        {
                            return Visibility.Visible;
                        }
                }

                return Visibility.Collapsed;
            }
        }
        public Visibility SelectionVisibility
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotHelp:
                    case MessageTypes.User:
                        {
                            return Visibility.Collapsed;
                        }
                    case MessageTypes.BotSelection:
                        {
                            return Visibility.Visible;
                        }
                }

                return Visibility.Collapsed;
            }
        }
        public HorizontalAlignment MessageHorizontalAlignment
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypes.User:
                        {
                            return HorizontalAlignment.Right;
                        }
                    case MessageTypes.BotHelp:
                    case MessageTypes.BotMessage:
                    case MessageTypes.BotSelection:
                        {
                            return HorizontalAlignment.Left;
                        }
                }

                return HorizontalAlignment.Center;
            }
        }

        #endregion

    }
}
