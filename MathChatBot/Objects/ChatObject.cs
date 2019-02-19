using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MathChatBot
{
    public class ChatObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private string message;
        private ChatMessageType messageType;

        private HorizontalAlignment messageHorizontalAlignment;
        private Brush messageBackground;
        private Brush messageForeground;

        public enum ChatMessageType
        {
            User,
            Bot                
        }

        public ChatObject(string message, ChatMessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("Message cannot be null");
                if (value == string.Empty)
                    throw new Exception("Message cannot be empty");
                    
                message = value;
                NotifyPropertyChanged(nameof(Message));
            }
        }

        public ChatMessageType MessageType
        {
            get { return messageType; }
            set
            {
                messageType = value;

                switch (messageType)
                {
                    case ChatMessageType.User:
                        {
                            var b = Application.Current.Resources["UserMessageColor"] as SolidColorBrush;
                            MessageBackground = Application.Current.Resources["UserMessageColor"] as SolidColorBrush;
                            MessageForeground = Application.Current.Resources["UserMessageTextColor"] as SolidColorBrush;
                            MessageHorizontalAlignment = HorizontalAlignment.Right;
                        }
                        break;
                    case ChatMessageType.Bot:
                        {
                            var b = Application.Current.Resources["BotMessageColor"] as SolidColorBrush;
                            MessageBackground = Application.Current.Resources["BotMessageColor"] as SolidColorBrush;
                            MessageForeground = Application.Current.Resources["BotMessageTextColor"] as SolidColorBrush;
                            MessageHorizontalAlignment = HorizontalAlignment.Left;
                        }
                        break;
                }

                NotifyPropertyChanged(nameof(MessageType));
            }
        }

        public HorizontalAlignment MessageHorizontalAlignment
        {
            get { return messageHorizontalAlignment; }
            private set
            {
                messageHorizontalAlignment = value;
                NotifyPropertyChanged(nameof(MessageHorizontalAlignment));
            }
        }

        public Brush MessageBackground
        {
            get { return messageBackground; }
            private set
            {
                messageBackground = value;
                NotifyPropertyChanged(nameof(MessageBackground));
            }
        }

        public Brush MessageForeground
        {
            get { return messageForeground; }
            private set
            {
                messageForeground = value;
                NotifyPropertyChanged(nameof(MessageForeground));
            }
        }


    }
}
