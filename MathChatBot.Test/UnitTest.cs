using MathChatBot.Helpers;
using MathChatBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest
    {

        #region Setup

        private static MathChatBotHelper helper;

        [AssemblyInitialize]
        public static void TestSetup(TestContext testContext)
        {
            DatabaseUtility.TestMode = true;
            helper = new MathChatBotHelper();
        }
        [AssemblyCleanup()]
        public static void TestCleanup()
        {
            DatabaseUtility.Entity.Dispose();
        }

        #endregion

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

        #region Entity

        [TestMethod]
        public void Entity_RemoveUserAndRelations()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var user = entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                var relations = user.UserRoleRelations.ToList();
                for (int i = relations.Count - 1; i >= 0; i--)
                {
                    entity.Entry(relations[i]).State = System.Data.Entity.EntityState.Deleted;
                }
                entity.SaveChanges();
                entity.Users.Remove(user);
                entity.SaveChanges();
                Assert.IsTrue(entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob") == null);
            }
        }
        [TestMethod]
        public void Entity_RemoveUserShouldCastExceptionBecauseOfExistingRelations()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var user = entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                    entity.Users.Remove(user);
                    entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_AddUser()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                entity.Users.Add(new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = EncryptUtility.Encrypt("test1234", DatabaseUtility.PassPhrase),
                    Username = "tete"
                });
                entity.SaveChanges();

                Assert.IsTrue(entity.Users.FirstOrDefault(x => x.FirstName == "Test" && x.Username == "tete") != null);
            }
        }

        [TestMethod]
        public void Entity_EditUser()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var user = entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                user.FirstName = "test";
                entity.SaveChanges();
                Assert.IsTrue(entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob") == null);
            }
        }

        #endregion

        #region MathChatBotHelper

        [TestMethod]
        public void MathChatBotHelper_AnalyzeTermWhichIsInTheDatabase()
        {
            helper.WriteMessageToBot("function");
            Assert.AreEqual("function", helper.LastBotMessage.Material.Term.Name.ToLower());
        }
        [TestMethod]
        public void MathChatBotHelper_GotDefinition()
        {
            helper.WriteMessageToBot("function");
            Assert.IsTrue(helper.LastBotMessage.IsMaterial);
        }
        [TestMethod]
        public void MathChatBotHelper_AnalyzeTermWhichIsInTheDatabaseLongSentence()
        {
            helper.WriteMessageToBot("cat, dog, cow, goat and acute triangle");
            Assert.AreEqual("acute triangle", helper.LastBotMessage.Material.Term.Name.ToLower());
        }
        [TestMethod]
        public void MathChatBotHelper_SeeExample()
        {
            helper.WriteMessageToBot("function");
            helper.WriteMessageToBot(Properties.Resources.see_example);
            Assert.IsTrue(helper.LastBotMessage.IsExample);
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
        public void MathChatBotHelper_SimpleCalculatorAdd()
        {
            helper.WriteMessageToBot("=0");
            helper.WriteMessageToBot("5");
            helper.WriteMessageToBot("8");
            helper.WriteMessageToBot("11");
            Assert.AreEqual("24", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorSubstraction()
        {
            helper.WriteMessageToBot("=12-89");
            Assert.AreEqual("-77", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorDivision()
        {
            helper.WriteMessageToBot("=1024/8");
            Assert.AreEqual("128", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorMultiply()
        {
            helper.WriteMessageToBot("=35*6");
            Assert.AreEqual("210", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorMultiple()
        {
            helper.WriteMessageToBot("=(35*6/10+6)-60");
            Assert.AreEqual("-33", helper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculatorSquareRoot()
        {
            helper.WriteMessageToBot("=sqrt(121)");
            Assert.AreEqual("11", helper.LastBotMessage.Text);
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
