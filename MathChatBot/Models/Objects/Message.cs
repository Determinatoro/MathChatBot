using System.Windows;
using System.Windows.Media;

namespace MathChatBot.Models
{
    public partial class Message
    {
        private const string MESSAGE_TYPE_USER = "User";
        private const string MESSAGE_TYPE_BOT = "Bot";

        public Brush MessageBackground
        {
            get
            {
                switch (MessageType.Name)
                {
                    case MESSAGE_TYPE_USER:
                        {
                            return Application.Current.Resources["UserMessageColor"] as SolidColorBrush;
                        }
                    case MESSAGE_TYPE_BOT:
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
                switch (MessageType.Name)
                {
                    case MESSAGE_TYPE_USER:
                        {
                            return Application.Current.Resources["UserMessageTextColor"] as SolidColorBrush;
                        }
                    case MESSAGE_TYPE_BOT:
                        {
                            return Application.Current.Resources["BotMessageTextColor"] as SolidColorBrush;
                        }
                }

                return new SolidColorBrush(Colors.Black);
            }
        }

        public HorizontalAlignment MessageHorizontalAlignment
        {
            get
            {
                switch (MessageType.Name)
                {
                    case MESSAGE_TYPE_USER:
                        {
                            return HorizontalAlignment.Right;
                        }
                    case MESSAGE_TYPE_BOT:
                        {
                            return HorizontalAlignment.Left;
                        }
                }

                return HorizontalAlignment.Center;
            }
        }
    }
}
