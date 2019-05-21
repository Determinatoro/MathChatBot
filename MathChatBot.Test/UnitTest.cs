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

        // User

        [TestMethod]
        public void Entity_RemoveUser()
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
                var user = entity.Users.FirstOrDefault(x => x.Username == "japr");
                user.FirstName = "Hans";
                entity.SaveChanges();
                Assert.IsTrue(entity.Users.FirstOrDefault(x => x.Username == "japr").FirstName == "Hans");
            }
        }

        // Class

        [TestMethod]
        public void Entity_AddClass()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var @class = entity.Classes.Add(new Models.Class()
                {
                    Name = "B999"
                });
                entity.SaveChanges();
                
                Assert.IsTrue(@class.Id != 0);
            }
        }
        [TestMethod]
        public void Entity_RemoveClassShouldCastExceptionBecauseTheClassHasRelationsToOtherTables()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var @class = entity.Classes.FirstOrDefault(x => x.Name == "B100");
                    entity.Entry(@class).State = System.Data.Entity.EntityState.Deleted;
                    entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveClass()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var @class = entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var relations = entity.UserClassRelations.Where(x => x.ClassId == @class.Id).ToList();
                relations.ForEach(x => entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                entity.SaveChanges();
                entity.Entry(@class).State = System.Data.Entity.EntityState.Deleted;
                entity.SaveChanges();
            }
        }
        [TestMethod]
        public void Entity_EditClass()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var @class = entity.Classes.FirstOrDefault(x => x.Name == "B100");
                @class.Name = "B999";
                entity.SaveChanges();
                Assert.IsTrue(entity.Classes.FirstOrDefault(x => x.Name == "B999") != null);
            }
        }

        // Role

        [TestMethod]
        public void Entity_AddRole()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var role = entity.Roles.Add(new Models.Role()
                {
                    Name = "Test"
                });
                entity.SaveChanges();
                Assert.IsTrue(role.Id != 0);
            }
        }
        [TestMethod]
        public void Entity_EditRole()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var role = entity.Roles.FirstOrDefault(x => x.Name == "Student");
                role.Name = "Officer";
                entity.SaveChanges();
                Assert.IsTrue(entity.Roles.FirstOrDefault(x => x.Name == "Student") == null);
            }
        }
        [TestMethod]
        public void Entity_RemoveRoleShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var role = entity.Roles.FirstOrDefault(x => x.Name == "Student");
                    entity.Entry(role).State = System.Data.Entity.EntityState.Deleted;
                    entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveRole()
        {
            using (var entity = DatabaseUtility.Entity)
            {
                var role = entity.Roles.FirstOrDefault(x => x.Name == "Student");
                var relations = entity.UserRoleRelations.Where(x => x.RoleId == role.Id).ToList();
                //relations.ForEach(x => )
                
                entity.Entry(role).State = System.Data.Entity.EntityState.Deleted;
                entity.SaveChanges();
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
