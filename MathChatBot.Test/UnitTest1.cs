using MathChatBot.Helpers;
using MathChatBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest1
    {
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

        #region MathChatBotHelper

        private static MathChatBotHelper helper;

        [AssemblyInitialize]
        public static void Setup(TestContext testContext)
        {
            DatabaseUtility.RunTest();
            var inMemoryContext = TestUtility.GetInMemoryContext();
            var users = inMemoryContext.Users.ToList();
            
            helper = new MathChatBotHelper();
            helper.User = users[0];
        }
        [AssemblyCleanup()]
        public static void ApplicationCleanup()
        {
            DatabaseUtility.Entity.Dispose();
        }

        [TestMethod]
        public void MathChatBotHelper_AnalyzeTermWhichIsInTheDatabase()
        {
            helper.WriteMessageToBot("function");
            Assert.AreEqual("function", helper.LastBotMessage.Material.Term.Name.ToLower());
        }
        [TestMethod]
        public void MathChatBotHelper_AnalyzeTermWhichIsNotInTheDatabase()
        {
            helper.WriteMessageToBot("plus");
            Assert.AreEqual(null, helper.LastBotMessage.Material);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorAddQuestion()
        {
            helper.WriteMessageToBot("What is 4 + 7");
            Assert.AreEqual("11", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorAddMultiple()
        {
            helper.WriteMessageToBot("=0");
            helper.WriteMessageToBot("5");
            helper.WriteMessageToBot("8");
            helper.WriteMessageToBot("11");
            Assert.AreEqual("24", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_NullText()
        {
            try
            {
                helper.WriteMessageToBot(null);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        #endregion

    }
}
