using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void MessageObjectConstructor_MessageIsNull_NullException()
        {
            ChatObject chatObject = new ChatObject(null, ChatObject.ChatMessageType.User);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MessageObjectConstructor_MessageIsEmpty_Exception()
        {
            var app = new App(); //magically sets Application.Current
            app.InitializeComponent();
            ChatObject chatObject = new ChatObject(string.Empty, ChatObject.ChatMessageType.User);
        }
    }
}
