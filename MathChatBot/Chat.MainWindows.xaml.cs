using System.Collections.Generic;
using System.Windows;

namespace MathChatBot
{
    public partial class MainWindow : Window
    {
        public List<ChatObject> ChatObjectList { get; set; }

        private void SetupChat()
        {
            ChatObjectList = new List<ChatObject>();
            ChatObjectList.Add(new ChatObject("Hi, how does it go with building a really good chatbot that works perfectly and helps everybody with math", ChatObject.ChatMessageType.Bot));
            ChatObjectList.Add(new ChatObject("It goes fine because programming is so fun", ChatObject.ChatMessageType.User));
            lbChat.ItemsSource = ChatObjectList;
        }

        private void AddChatObject(string message)
        {
            if (message == "")
                return;
            ChatObjectList.Add(new ChatObject(message, ChatObject.ChatMessageType.User));
            lbChat.Items.Refresh();            
        }
        
    }
}
