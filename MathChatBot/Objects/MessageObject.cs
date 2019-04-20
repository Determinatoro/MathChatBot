using MathChatBot.Models;
using MathChatBot.Utilities;
using System;
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
        User
    }

    public class MessageObject
    {
        private Term _term;
        private Topic _topic;
        private bool _isExample;

        public MessageObject()
        {
            SendDate = DateTime.Now;
        }

        public MessageObject(string text)
        {
            Text = text;
            MessageType = MessageTypes.BotMessage;
        }

        public MessageObject(Material material, int showOrderId) : this()
        {
            Material = material;
            ShowOrderId = showOrderId;
            IsExample = true;
        }

        public bool IsExample
        {
            get { return _isExample; }
            private set
            {
                _isExample = value;
                if (value)
                {
                    _topic = null;

                    MaterialExample = Material.MaterialExamples.FirstOrDefault(x => x.ShowOrderId == (ShowOrderId == null ? 1 : ShowOrderId));
                    Source = MaterialExample.Source;
                }
                else
                {
                    MaterialExample = null;
                }
            }
        }
        public int? ShowOrderId { get; set; }
        public MaterialExample MaterialExample { get; set; }
        public string Source { get; private set; }
        public System.DateTime SendDate { get; set; }
        public string Text { get; set; }
        public MessageTypes MessageType { get; set; }
        public Term Term
        {
            get { return _term; }
            set
            {
                _term = value;
                if (_term != null)
                {
                    _topic = null;

                    Material = Term.Materials.FirstOrDefault(x => x.ShowOrderId == 1);
                    Source = Material.Source;
                }
            }
        }
        public bool ShowExampleButton
        {
            get
            {
                if (IsExample)
                    return false;
                return Material != null && Material.MaterialExamples.Count != 0;
            }
        }
        public Topic Topic
        {
            get { return _topic; }
            set
            {
                _topic = value;
                if (_topic != null)
                {
                    _term = null;

                    Material = Topic.Materials.FirstOrDefault(x => x.ShowOrderId == 1);
                    Source = Material?.Source;
                }
            }
        }
        public Material Material { get; set; }
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
                        {
                            return HorizontalAlignment.Left;
                        }
                }

                return HorizontalAlignment.Center;
            }
        }
        public bool IsTopic
        {
            get
            {
                return Topic != null;
            }
        }
        public bool IsTerm
        {
            get
            {
                return Term != null && !IsExample;
            }
        }

    }
}
