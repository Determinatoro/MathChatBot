using MathChatBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest1
    {
        private void InitializeComponent()
        {
            var app = new App();
            app.InitializeComponent();
        }

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
            InitializeComponent();
            ChatObject chatObject = new ChatObject(string.Empty, ChatObject.ChatMessageType.User);
        }

        #region EncryptionUtility

        [TestMethod]       
        public void EncryptUtility_Decrypt_Equal()
        {
            var decryptedText = EncryptUtility.Decrypt("XFRGhQoc3+jj+qQFWhe2vpaRH8q1Pyj1X8NddCoSJcuYXxXisTldPPMq9ynI2doGDQD7ApvSQOJxhK1SDZ0R2c+4pFTR7aVz7Ew3F2UvDpeHNBnOBiiwmMSBq8pF8exs", "test1234");            
            Assert.AreEqual("test1234", decryptedText);
        }

        [TestMethod]
        public void EncryptUtility_Encrypt_Equal()
        {
            var encryptedText = EncryptUtility.Encrypt("test1234", "test1234");
            var decryptedText = EncryptUtility.Decrypt(encryptedText, "test1234");
            Assert.AreEqual("test1234", decryptedText);
        }

        #endregion

    }
}
