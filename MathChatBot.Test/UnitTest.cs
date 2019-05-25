using MathChatBot.Helpers;
using MathChatBot.Models;
using MathChatBot.Objects;
using MathChatBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using static MathChatBot.Models.Role;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest
    {

        //*************************************************/
        // VARIABLES
        //*************************************************/
        #region Variables

        private static MathChatBotHelper MathChatBotHelper;
        private static SimpleCalculatorHelper SimpleCalculatorHelper;
        private static MathChatBotEntities Entity { get { return DatabaseUtility.Entity; } }

        #endregion

        //*************************************************/
        // SETUP
        //*************************************************/
        #region Setup

        [AssemblyInitialize]
        public static void TestSetup(TestContext testContext)
        {
            DatabaseUtility.TestMode = true;
            MathChatBotHelper = new MathChatBotHelper();
            SimpleCalculatorHelper = new SimpleCalculatorHelper();
        }
        [AssemblyCleanup()]
        public static void TestCleanup()
        {
            DatabaseUtility.Entity.Dispose();
        }

        #endregion

        //*************************************************/
        // ENCRYPTION UTILITY
        //*************************************************/
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

        //*************************************************/
        // ENTITY
        //*************************************************/
        #region Entity

        // User

        [TestMethod]
        public void Entity_RemoveUser()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                var relations = user.UserRoleRelations.ToList();
                for (int i = relations.Count - 1; i >= 0; i--)
                {
                    Entity.Entry(relations[i]).State = System.Data.Entity.EntityState.Deleted;
                }
                Entity.SaveChanges();
                Entity.Users.Remove(user);
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob") == null);
            }
        }
        [TestMethod]
        public void Entity_RemoveUserShouldCastExceptionBecauseOfExistingRelations()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var user = Entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                    Entity.Users.Remove(user);
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_AddUser()
        {
            using (Entity)
            {
                Entity.Users.Add(new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = EncryptUtility.Encrypt("test1234", DatabaseUtility.PassPhrase),
                    Username = "tete"
                });
                Entity.SaveChanges();

                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.FirstName == "Test" && x.Username == "tete") != null);
            }
        }
        [TestMethod]
        public void Entity_EditUser()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                user.FirstName = "Hans";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.Username == "japr").FirstName == "Hans");
            }
        }

        // Class

        [TestMethod]
        public void Entity_AddClass()
        {
            using (Entity)
            {
                var @class = Entity.Classes.Add(new Models.Class()
                {
                    Name = "B999"
                });
                Entity.SaveChanges();

                Assert.IsTrue(@class.Id != 0);
            }
        }
        [TestMethod]
        public void Entity_RemoveClassShouldCastExceptionBecauseTheClassHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                    Entity.Entry(@class).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveClass()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var relations = Entity.UserClassRelations.Where(x => x.ClassId == @class.Id).ToList();
                relations.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                Entity.Entry(@class).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
            }
        }
        [TestMethod]
        public void Entity_EditClass()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                @class.Name = "B999";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Classes.FirstOrDefault(x => x.Name == "B999") != null);
            }
        }

        // Role

        [TestMethod]
        public void Entity_AddRole()
        {
            using (Entity)
            {
                var role = Entity.Roles.Add(new Models.Role()
                {
                    Name = "Test"
                });
                Entity.SaveChanges();
                Assert.IsTrue(role.Id != 0);
            }
        }
        [TestMethod]
        public void Entity_EditRole()
        {
            using (Entity)
            {
                var role = Entity.Roles.FirstOrDefault(x => x.Name == "Student");
                role.Name = "Officer";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Roles.FirstOrDefault(x => x.Name == "Student") == null);
            }
        }
        [TestMethod]
        public void Entity_RemoveRoleShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var role = Entity.Roles.FirstOrDefault(x => x.Name == "Student");
                    Entity.Entry(role).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveRole()
        {
            using (Entity)
            {
                var role = Entity.Roles.FirstOrDefault(x => x.Name == "Student");
                var relations = Entity.UserRoleRelations.Where(x => x.RoleId == role.Id).ToList();
                relations.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                Entity.Entry(role).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
            }
        }

        // HelpRequest

        [TestMethod]
        public void Entity_RemoveHelpRequest()
        {
            using (Entity)
            {
                var helpRequest = Entity.HelpRequests.FirstOrDefault();
                Entity.Entry(helpRequest).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                Assert.IsTrue(Entity.HelpRequests.Where(x => x.Id == helpRequest.Id).FirstOrDefault() == null);
            }
        }
        [TestMethod]
        public void Entity_AddHelpRequest()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var example = Entity.MaterialExamples.FirstOrDefault(x => x.Material.Term.Name.ToLower() == "cosine");
                Entity.HelpRequests.Add(new Models.HelpRequest()
                {
                    UserId = user.Id,
                    TermId = example.Material.TermId.Value,
                    MaterialExampleId = example.Id
                });
                Entity.SaveChanges();
                Assert.IsTrue(Entity.HelpRequests.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine") != null);
            }
        }

        // Material

        [TestMethod]
        public void Entity_AddMaterial()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "sine");

                Entity.Materials.Add(new Models.Material()
                {
                    TermId = term.Id,
                    Source = "1234567890",
                    ShowOrderId = 1
                });
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Materials.FirstOrDefault(x => x.Source == "1234567890") != null);
            }
        }
        [TestMethod]
        public void Entity_RemoveMaterialShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                    Entity.Entry(material).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveMaterial()
        {
            using (Entity)
            {
                var examples = Entity.MaterialExamples.Where(x => x.Material.Term.Name.ToLower() == "cosine").ToList();
                examples.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                Entity.Entry(material).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine") == null);
            }
        }
        [TestMethod]
        public void Entity_EditMaterial()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                material.Source = "test";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Materials.FirstOrDefault(x => x.Source == "test") != null);
            }
        }

        // MaterialExample

        [TestMethod]
        public void Entity_AddMaterialExample()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");

                Entity.MaterialExamples.Add(new Models.MaterialExample()
                {
                    MaterialId = material.Id,
                    Source = "test",
                    ShowOrderId = 1
                });
                Entity.SaveChanges();
                Assert.IsTrue(Entity.MaterialExamples.FirstOrDefault(x => x.Source == "test") != null);
            }
        }
        [TestMethod]
        public void Entity_RemoveMaterialExampleShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var example = Entity.MaterialExamples.FirstOrDefault(x => x.Material.Term.Name.ToLower() == "sine");
                    Entity.Entry(example).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveMaterialExample()
        {
            using (Entity)
            {
                var helpRequest = Entity.HelpRequests.ToList();
                helpRequest.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var example = Entity.MaterialExamples.FirstOrDefault(x => x.Material.Term.Name.ToLower() == "sine");
                Entity.Entry(example).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
            }
        }
        [TestMethod]
        public void Entity_EditMaterialExample()
        {
            using (Entity)
            {
                var example = Entity.MaterialExamples.FirstOrDefault(x => x.Material.Term.Name.ToLower() == "sine");
                example.Source = "test";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.MaterialExamples.FirstOrDefault(x => x.Source == "test") != null);
            }
        }

        // Assignment

        [TestMethod]
        public void Entity_AddAssignment()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "sine");
                Entity.Assignments.Add(new Models.Assignment()
                {
                    AnswerA = "1234",
                    AssignmentNo = 2,
                    TermId = term.Id,
                    Source = "test"
                });
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Assignments.FirstOrDefault(x => x.Source == "test" && x.Term.Name.ToLower() == "sine") != null);
            }
        }
        [TestMethod]
        public void Entity_RemoveAssignment()
        {
            using (Entity)
            {
                var assignment = Entity.Assignments.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                Entity.Entry(assignment).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Assignments.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine") == null);
            }
        }
        [TestMethod]
        public void Entity_EditAssignment()
        {
            using (Entity)
            {
                var assignment = Entity.Assignments.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                assignment.Source = "test";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Assignments.FirstOrDefault(x => x.Source == "test") != null);
            }
        }

        // Term

        [TestMethod]
        public void Entity_AddTerm()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault();
                Entity.Terms.Add(new Models.Term()
                {
                    TopicId = topic.Id,
                    Name = "test"
                });
                Entity.SaveChanges();

                Assert.IsTrue(Entity.Terms.FirstOrDefault(x => x.Name == "test") != null);
            }
        }
        [TestMethod]
        public void Entity_RemoveTermShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var term = Entity.Terms.FirstOrDefault();
                    Entity.Entry(term).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveTerm()
        {
            using (Entity)
            {
                var examples = Entity.MaterialExamples.Where(x => x.Material.Term.Name.ToLower() == "cosine").ToList();
                examples.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var materials = Entity.Materials.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                var assignments = Entity.Assignments.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                materials.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                assignments.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                Entity.Entry(term).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Terms.FirstOrDefault(x => x.Name == "cosine") == null);
            }
        }
        [TestMethod]
        public void Entity_EditTerm()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                term.Name = "test";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Terms.FirstOrDefault(x => x.Name == "cosine") == null);
            }
        }

        // Topic

        [TestMethod]
        public void Entity_AddTopic()
        {
            using (Entity)
            {
                Entity.Topics.Add(new Models.Topic()
                {
                    Name = "test"
                });
                Entity.SaveChanges();

                Assert.IsTrue(Entity.Topics.FirstOrDefault(x => x.Name == "test") != null);
            }
        }
        [TestMethod]
        public void Entity_RemoveTopicShouldCastExceptionAsItHasRelationsToOtherTables()
        {
            using (Entity)
            {
                Assert.ThrowsException<DbUpdateException>(() =>
                {
                    var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == "trigonometry");
                    Entity.Entry(topic).State = System.Data.Entity.EntityState.Deleted;
                    Entity.SaveChanges();
                });
            }
        }
        [TestMethod]
        public void Entity_RemoveTopic()
        {
            using (Entity)
            {
                var topicName = "probability";
                var examples = Entity.MaterialExamples.Where(x => x.Material.Term.Topic.Name.ToLower() == topicName).ToList();
                examples.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var materials = Entity.Materials.Where(x => x.Term.Topic.Name.ToLower() == topicName || x.Topic.Name.ToLower() == topicName).ToList();
                var assignments = Entity.Assignments.Where(x => x.Term.Topic.Name.ToLower() == topicName).ToList();
                materials.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                assignments.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var terms = Entity.Terms.Where(x => x.Topic.Name.ToLower() == topicName).ToList();
                terms.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == topicName);
                Entity.Entry(topic).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Topics.FirstOrDefault(x => x.Name == topicName) == null);
            }
        }
        [TestMethod]
        public void Entity_EditTopic()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == "trigonometry");
                topic.Name = "test";
                Entity.SaveChanges();
                Assert.IsTrue(Entity.Terms.FirstOrDefault(x => x.Name == "trigonometry") == null);
            }
        }

        #endregion

        //*************************************************/
        // DATABASE UTILITY
        //*************************************************/
        #region DatabaseUtility

        [TestMethod]
        public void DatabaseUtility_GenerateUsername()
        {
            using (Entity)
            {
                var username = Entity.GenerateUsername("Akeem", "Rose");
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.Username == username) == null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GenerateUsername_ReturnNullBecauseNamesAreLessThanTwoLettersLong()
        {
            using (Entity)
            {
                var username = Entity.GenerateUsername("A", "R");
                Assert.IsTrue(username == null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUserFromUsername()
        {
            using (Entity)
            {
                var user = Entity.GetUserFromUsername("japr");
                Assert.IsTrue(user != null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CheckLogin()
        {
            using (Entity)
            {
                var response = Entity.CheckLogin("japr", "test1234");
                Assert.IsTrue(response.Success);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CheckLogin_UsernameNotFound()
        {
            using (Entity)
            {
                var response = Entity.CheckLogin("test", "test1234");
                Assert.IsTrue(!response.Success && response.ErrorMessage == Properties.Resources.wrong_username_or_password);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CheckLogin_PasswordWrong()
        {
            using (Entity)
            {
                var response = Entity.CheckLogin("japr", "test");
                Assert.IsTrue(!response.Success && response.ErrorMessage == Properties.Resources.wrong_username_or_password);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CheckLogin_UserDeactivated()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                user.IsActivated = false;
                Entity.SaveChanges();
                var response = Entity.CheckLogin("japr", "test1234");
                Assert.IsTrue(!response.Success && response.ErrorMessage == Properties.Resources.user_is_deactivated_contact_your_administrator);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUserRoles()
        {
            using (Entity)
            {
                var roles = Entity.GetUserRoles("japr");
                Assert.IsTrue(roles.Count == 1 && roles.Any(x => x == Role.RoleTypes.Administrator));
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUserRoles_UserNotFound()
        {
            using (Entity)
            {
                var roles = Entity.GetUserRoles("test");
                Assert.IsTrue(roles == null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_UpdateUserInformation()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var roles = Entity.GetRolesListForUser(user.Username);
                roles = roles.OrderBy(x => x.Name).ToList();
                roles.FirstOrDefault(x => x.Name == RoleTypes.Administrator.GetName()).IsAssigned = false;
                roles.FirstOrDefault(x => x.Name == RoleTypes.Student.GetName()).IsAssigned = true;
                roles.FirstOrDefault(x => x.Name == RoleTypes.Teacher.GetName()).IsAssigned = true;
                Entity.UpdateUserInformation(user, roles, false);
                roles = Entity.GetRolesListForUser(user.Username);
                Assert.IsTrue(roles.Any(x => x.Name == RoleTypes.Teacher.GetName()) && roles.Any(x => x.Name == RoleTypes.Student.GetName()));
            }
        }
        [TestMethod]
        public void DatabaseUtility_UpdateUserInformationDoNotDeleteRole()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var roles = Entity.GetRolesListForUser(user.Username);
                roles.FirstOrDefault(x => x.Name == RoleTypes.Administrator.GetName()).IsAssigned = true;
                roles.FirstOrDefault(x => x.Name == RoleTypes.Student.GetName()).IsAssigned = false;
                roles.FirstOrDefault(x => x.Name == RoleTypes.Teacher.GetName()).IsAssigned = false;
                Entity.UpdateUserInformation(user, roles, false);
                roles = Entity.GetRolesListForUser(user.Username);
                Assert.IsTrue(roles.Any(x => x.Name == RoleTypes.Administrator.GetName()));
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersOrderedAlphabetically()
        {
            using (Entity)
            {
                var users = Entity.GetUsersOrderedAlphabetically();
                Assert.IsTrue(users[0].FirstName == "Aaron" && users[1].FirstName == "Abdul");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetClassesOrderedAlphabetically()
        {
            using (Entity)
            {
                var classes = Entity.GetClassesOrderedAlphabetically();
                Assert.IsTrue(classes[0].Name == "B100" && classes[1].Name == "B200");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetRolesListForUser()
        {
            using (Entity)
            {
                var roles = Entity.GetRolesListForUser("japr");
                var role = roles.FirstOrDefault(x => x.RoleType == RoleTypes.Administrator);
                Assert.IsTrue(role.IsAssigned);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CreateUser()
        {
            using (Entity)
            {
                var user = new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = "test1234",
                    Username = "tete"
                };

                user = Entity.CreateUser(user, disposeAfterwards: false);
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.FirstName == "Test") != null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_CreateUser_AssignRoles()
        {
            using (Entity)
            {
                var user = new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = "test1234",
                    Username = "tete"
                };
                var roles = Entity.Roles.ToList();
                roles.FirstOrDefault(x => x.Name == RoleTypes.Administrator.GetName()).IsAssigned = true;

                user = Entity.CreateUser(user, roles, disposeAfterwards: false);
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.FirstName == "Test") != null && Entity.UserRoleRelations.Any(x => x.UserId == user.Id));
            }
        }
        [TestMethod]
        public void DatabaseUtility_CreateUser_UserAlreadyExist()
        {
            using (Entity)
            {
                var user = new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = "test1234",
                    Username = "japr"
                };

                user = Entity.CreateUser(user, disposeAfterwards: false);
                Assert.IsTrue(Entity.Users.FirstOrDefault(x => x.FirstName == "Test") == null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddToClassesByName()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var classes = new HashSet<string>();
                classes.Add("TEST100");
                Entity.AddToClassesByName(user, classes, false);
                Assert.IsTrue(!Entity.Classes.Any(x => x.Name == "TEST100"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddToClassesByName_Administrator()
        {
            using (Entity)
            {
                var classes = new HashSet<string>();
                classes.Add("TEST100");
                Entity.AddToClassesByName("japr", classes, false);

                Assert.IsTrue(!Entity.Classes.Any(x => x.Name == "TEST100"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddToClassesByName_Student()
        {
            using (Entity)
            {
                var classes = new HashSet<string>();
                classes.Add("TEST100");
                var relations = Entity.UserClassRelations.Where(x => x.User.Username == "akro20").ToList();
                relations.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var success = Entity.AddToClassesByName("akro20", classes, false);
                Assert.IsTrue(success && Entity.Classes.Any(x => x.Name == "TEST100"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AssignRolesByName()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var roles = new HashSet<string>();
                roles.Add("Student");
                Entity.SaveChanges();
                var success = Entity.AssignRolesByName(user, roles, false);
                var userRoles = Entity.GetUserRoles(user.Username);
                Assert.IsTrue(userRoles.Any(x => x == RoleTypes.Student));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AssignRolesByName_Student()
        {
            using (Entity)
            {
                var roles = new HashSet<string>();
                roles.Add("Teacher");
                Entity.SaveChanges();
                var success = Entity.AssignRolesByName("akro20", roles, false);
                var userRoles = Entity.GetUserRoles("akro20");
                Assert.IsTrue(userRoles.Any(x => x == RoleTypes.Teacher));
            }
        }
        [TestMethod]
        public void DatabaseUtility_ResetPassword()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                Entity.ResetUserPassword("tiger1234", user.Username);
                user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                Assert.IsTrue(EncryptUtility.Decrypt(user.Password, DatabaseUtility.PassPhrase) == "tiger1234");
            }
        }
        [TestMethod]
        public void DatabaseUtility_ResetPassword_HasToBe8CharsLong()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var response = Entity.ResetUserPassword("tiger", user.Username);
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.the_password_has_to_be_at_least_eight_characters_long);
            }
        }
        [TestMethod]
        public void DatabaseUtility_ResetPassword_UserNotFound()
        {
            using (Entity)
            {
                var response = Entity.ResetUserPassword("tiger1234", "test");
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.user_not_found);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTermNames()
        {
            using (Entity)
            {
                var termNames = Entity.GetTermNames("trigonometry");
                var termNames2 = Entity.Terms.Where(x => x.Topic.Name == "trigonometry").ToList();
                Assert.IsTrue(termNames.Count == termNames2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTopicName()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Topic.Name.ToLower() == "trigonometry");
                var topicName = Entity.GetTopicName(material);
                Assert.IsTrue(topicName.ToLower() == "trigonometry");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTermName_ForMaterial()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var termName = Entity.GetTermName(material);
                Assert.IsTrue(termName.ToLower() == "sine");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTermName_ForMaterialExample()
        {
            using (Entity)
            {
                var materialExample = Entity.MaterialExamples.FirstOrDefault(x => x.Material.Term.Name.ToLower() == "sine");
                var termName = Entity.GetTermName(materialExample: materialExample);
                Assert.IsTrue(termName.ToLower() == "sine");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTermName_ForAssignment()
        {
            using (Entity)
            {
                var assignment = Entity.Assignments.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var termName = Entity.GetTermName(assignment: assignment);
                Assert.IsTrue(termName.ToLower() == "sine");
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTopicNames()
        {
            using (Entity)
            {
                var topicNames = Entity.GetTopicNames();
                var topicNames2 = Entity.Topics.ToList();
                Assert.IsTrue(topicNames.Count == topicNames2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddUsersToClass()
        {
            using (Entity)
            {
                var user = new Models.User()
                {
                    FirstName = "Test",
                    LastName = "Test",
                    Password = "test1234",
                    Username = "tete"
                };
                var roles = Entity.Roles.ToList();
                roles.FirstOrDefault(x => x.Name == RoleTypes.Student.GetName()).IsAssigned = true;
                user = Entity.CreateUser(user, roles, false);
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");

                var tempList = new List<User>();
                tempList.Add(user);
                Entity.AddUsersToClass(@class, tempList, false);
                var users = Entity.GetUsersInClass(@class, new Role.RoleTypes[] { Role.RoleTypes.Student, Role.RoleTypes.Teacher }, false);

                Assert.IsTrue(users.Any(x => x.FirstName == "Test"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddClass()
        {
            using (Entity)
            {
                var response = Entity.AddClass("TEST100", false);
                Assert.IsTrue(response.Success);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddClass_ClassAlreadyExist()
        {
            using (Entity)
            {
                var response = Entity.AddClass("B100", false);
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.the_class_already_exists);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersInClass()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var roleTypes = new Role.RoleTypes[] { Role.RoleTypes.Student, Role.RoleTypes.Teacher };
                var users = Entity.GetUsersInClass(@class, roleTypes, true);
                var roles = roleTypes.Select(x => x.GetName()).ToList();
                var usersInClass = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id) && x.UserRoleRelations.Any(x2 => roles.Contains(x2.Role.Name)))
                    .ToList();

                Assert.IsTrue(users.Count == usersInClass.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersInClass_All()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var users = Entity.GetUsersInClass(@class, null, true);
                var usersInClass = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id))
                    .ToList();

                Assert.IsTrue(users.Count == usersInClass.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersWithHelpRequests()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var users = Entity.GetUsersWithHelpRequests(@class, null);
                Assert.IsTrue(users.Count == 1);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersWithHelpRequests_ForTopic()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B200");
                var users = Entity.GetUsersWithHelpRequests(@class, "Trigonometry");
                Assert.IsTrue(users.Count == 0);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetHelpRequestsFromUsers()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var users = Entity.GetUsersInClass(@class, null, false);
                var helpRequests = Entity.GetHelpRequestsFromUsers(users, null);
                Assert.IsTrue(helpRequests.Count == 1);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetHelpRequestsFromUsersSlow()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var users = Entity.GetUsersInClass(@class, null, false);
                var helpRequests = Entity.GetHelpRequestsFromUsersSlow(users, null);
                Assert.IsTrue(helpRequests.Count == 1);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetHelpRequestsFromUser()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "akro20");
                var helpRequests = Entity.GetHelpRequestsFromUser(user, null);
                Assert.IsTrue(helpRequests.Count == 1);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetUsersNotInClass()
        {
            using (Entity)
            {
                var @class = Entity.Classes.FirstOrDefault(x => x.Name == "B100");
                var users = Entity.GetUsersNotInClass(@class);
                Assert.IsTrue(users.Any(x => x.FirstName == "Aaron"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetTermInformations()
        {
            using (Entity)
            {
                var terms = Entity.GetTermInformations("Trigonometry");
                var term = terms.FirstOrDefault(x => x.Name == "Sine");
                Assert.IsTrue(term.AssignmentsCount == 2 && term.ExamplesCount == 1 && term.MaterialsCount == 1);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddNewTopic()
        {
            using (Entity)
            {
                Entity.AddNewTopic("Test", false);
                Assert.IsTrue(Entity.Topics.Any(x => x.Name == "Test"));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddNewTopic_TopicAlreadyExist()
        {
            using (Entity)
            {
                var flag = Entity.AddNewTopic("Trigonometry", false);
                Assert.IsTrue(flag);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddNewTerm()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name == "Trigonometry");
                var flag = Entity.AddNewTerm(topic, "Test", false);
                Assert.IsTrue(flag);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddNewTerm_TermAlreadyExist()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name == "Trigonometry");
                var flag = Entity.AddNewTerm(topic, "Sine", false);
                Assert.IsTrue(flag);
            }
        }
        [TestMethod]
        public void DatabaseUtility_DeleteTopic_StillHasRelations()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name == "Trigonometry");
                var flag = Entity.DeleteTopic(topic);
                Assert.IsTrue(!flag);
            }
        }
        [TestMethod]
        public void Entity_DeleteTopic()
        {
            using (Entity)
            {
                var topicName = "probability";
                var examples = Entity.MaterialExamples.Where(x => x.Material.Term.Topic.Name.ToLower() == topicName).ToList();
                examples.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var materials = Entity.Materials.Where(x => x.Term.Topic.Name.ToLower() == topicName || x.Topic.Name.ToLower() == topicName).ToList();
                var assignments = Entity.Assignments.Where(x => x.Term.Topic.Name.ToLower() == topicName).ToList();
                materials.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                assignments.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var terms = Entity.Terms.Where(x => x.Topic.Name.ToLower() == topicName).ToList();
                terms.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == topicName);
                Entity.DeleteTopic(topic);
                Assert.IsTrue(Entity.Topics.FirstOrDefault(x => x.Name == topicName) == null);
            }
        }
        [TestMethod]
        public void DatabaseUtility_DeleteTerm()
        {
            using (Entity)
            {
                var examples = Entity.MaterialExamples.Where(x => x.Material.Term.Name.ToLower() == "cosine").ToList();
                examples.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var materials = Entity.Materials.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                var assignments = Entity.Assignments.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                materials.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                assignments.ForEach(x => Entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                Entity.SaveChanges();
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                var flag = Entity.DeleteTerm(term);
                Entity.SaveChanges();
                Assert.IsTrue(flag);
            }
        }
        [TestMethod]
        public void DatabaseUtility_DeleteTerm_StillHasRelations()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name == "Cosine");
                var flag = Entity.DeleteTerm(term);
                Entity.SaveChanges();
                Assert.IsTrue(!flag);
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddTopicMaterial()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name == "Trigonometry");
                Entity.AddTopicMaterial(topic, "test", false);
                
                Assert.IsTrue(Entity.Materials.Any(x => x.Source == "test" && x.TopicId == topic.Id));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddTermMaterial()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name == "Sine");
                Entity.AddTermMaterial(term, "test", false);

                Assert.IsTrue(Entity.Materials.Any(x => x.Source == "test" && x.TermId == term.Id));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddTermMaterialExample()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name == "Sine");
                Entity.AddTermMaterialExample(material, "test", false);

                Assert.IsTrue(Entity.MaterialExamples.Any(x => x.Source == "test" && x.Material.TermId == material.TermId));
            }
        }
        [TestMethod]
        public void DatabaseUtility_AddAssignment()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                Entity.AddAssignment(term, "test", null, null, null, null, null, null, null, false);
                Assert.IsTrue(Entity.Assignments.Any(x => x.Source == "test" && x.TermId == term.Id));
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetExamples()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name == "Sine");
                var examples = Entity.MaterialExamples.Where(x => x.MaterialId == material.Id).ToList();
                var examples2 = Entity.GetExamples(material.Id);
                Assert.IsTrue(examples.Count == examples2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetMaterials_ForTerm()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                var materials = Entity.Materials.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                var materials2 = Entity.GetMaterials(term.Id);
                Assert.IsTrue(materials.Count == materials2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetMaterials_ForTopic()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == "trigonometry");
                var materials = Entity.Materials.Where(x => x.Topic.Name.ToLower() == "trigonometry").ToList();
                var materials2 = Entity.GetMaterials(topicId: topic.Id);
                Assert.IsTrue(materials.Count == materials2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetAssignments_ForTerm()
        {
            using (Entity)
            {
                var term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == "cosine");
                var assignments = Entity.Assignments.Where(x => x.Term.Name.ToLower() == "cosine").ToList();
                var assignments2 = Entity.GetAssignments(term.Id);
                Assert.IsTrue(assignments.Count == assignments2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetAssignments_ForTopic()
        {
            using (Entity)
            {
                var topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == "trigonometry");
                var assignments = Entity.Assignments.Where(x => x.Term.Topic.Name.ToLower() == "trigonometry").ToList();
                var assignments2 = Entity.GetAssignments(null, topic.Id);
                Assert.IsTrue(assignments.Count == assignments2.Count);
            }
        }
        [TestMethod]
        public void DatabaseUtility_MakeHelpRequest_Administrator()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "japr");
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var helpRequests = Entity.HelpRequests.ToList();
                var response = Entity.MakeHelpRequest(user.Id, material.TermId, material.Id, null, null, false);
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.you_need_to_be_a_student_to_make_help_requests);
            }
        }
        [TestMethod]
        public void DatabaseUtility_MakeHelpRequest_Student()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "akro20");
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                var helpRequests = Entity.HelpRequests.ToList();
                var response = Entity.MakeHelpRequest(user.Id, material.TermId, material.Id, null, null, false);
                Assert.IsTrue(response.Success);
            }
        }
        [TestMethod]
        public void DatabaseUtility_MakeHelpRequest_AlreadyMade()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "akro20");
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var response = Entity.MakeHelpRequest(user.Id, material.TermId, material.Id, null, null, false);
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.you_have_already_sent_a_help_request_for_this_material);
            }
        }
        [TestMethod]
        public void DatabaseUtility_MakeHelpRequest_NoTermId()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.Username == "akro20");
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var helpRequests = Entity.HelpRequests.ToList();
                var response = Entity.MakeHelpRequest(user.Id, null, material.Id, null, null, false);
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.could_not_make_a_help_request);
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetHelpRequestSources()
        {
            using (Entity)
            {
                var material = Entity.Materials.FirstOrDefault(x => x.Term.Name.ToLower() == "sine");
                var response = Entity.GetHelpRequestSources("Sine");
                var sources = (List<SourceObject>)response.Data;
                Assert.IsTrue(sources.Any(x => x.Source == material.Source));
            }
        }
        [TestMethod]
        public void DatabaseUtility_GetHelpRequestSources_NoSources()
        {
            using (Entity)
            {
                var response = Entity.GetHelpRequestSources("Numbers");
                Assert.IsTrue(response.ErrorMessage == Properties.Resources.no_help_requests_for_this_term);
            }
        }

        #endregion

        //*************************************************/
        // MATHCHATBOT HELPER
        //*************************************************/
        #region MathChatBotHelper

        [TestMethod]
        public void MathChatBotHelper_TermWhichIsInTheDatabase()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            Assert.AreEqual("function", MathChatBotHelper.LastBotMessage.Material.Term.Name.ToLower());
        }
        [TestMethod]
        public void MathChatBotHelper_Reset()
        {
            MathChatBotHelper.Reset();
            Assert.IsTrue(MathChatBotHelper.LastBotMessage == null);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeTopics()
        {
            using (Entity)
            {
                var topics = Entity.GetTopicNames();
                var joined = string.Join("\n", topics);
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.topics);
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text.Contains(joined));
            }
        }
        [TestMethod]
        public void MathChatBotHelper_SeeTerms()
        {
            using (Entity)
            {
                MathChatBotHelper.WriteMessageToBot("trigonometry");
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_terms);
                var terms = Entity.GetTermNames("trigonometry");
                var joined = string.Join("\n", terms);
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text.Contains(joined));
            }
        }
        [TestMethod]
        public void MathChatBotHelper_Terms()
        {
            MathChatBotHelper.WriteMessageToBot("trigonometry");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.terms);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_Help()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.help);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.help_message);
        }
        [TestMethod]
        public void MathChatBotHelper_Hello()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.hello);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.hello_response);
        }
        [TestMethod]
        public void MathChatBotHelper_WhatIsYourName()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.what_is_your_name);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.what_is_your_name_response);
        }
        [TestMethod]
        public void MathChatBotHelper_WhatIsTheMeaningOfLife()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.what_is_the_meaning_of_life);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.what_is_the_meaning_of_life_response);
        }
        [TestMethod]
        public void MathChatBotHelper_TellMeAJoke()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.tell_me_a_joke);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.tell_me_a_joke_response);
        }
        [TestMethod]
        public void MathChatBotHelper_WhoIsYourCreator()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.who_is_your_creator);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.who_is_your_creator_response);
        }
        [TestMethod]
        public void MathChatBotHelper_AnyNews()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.any_news);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.any_news_response);
        }
        [TestMethod]
        public void MathChatBotHelper_DoesGodExist()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.does_god_exist);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.does_god_exist_response);
        }
        [TestMethod]
        public void MathChatBotHelper_TermsNoTopicDefinitionSelected()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.terms);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeTermsNoTopicSelected()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_terms);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text != null);
        }
        [TestMethod]
        public void MathChatBotHelper_RunCommandSeeExample()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            MathChatBotHelper.RunCommand(Properties.Resources.see_example, MathChatBotHelper.LastBotMessage);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsExample);
        }
        [TestMethod]
        public void MathChatBotHelper_RunCommandSeeExampleDefinitionNotSelected()
        {
            MathChatBotHelper.WriteMessageToBot("trigonometry");
            MathChatBotHelper.RunCommand(Properties.Resources.see_example, MathChatBotHelper.LastBotMessage);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_RunCommandSeeExampleButHasNoExamples()
        {
            MathChatBotHelper.WriteMessageToBot("numbers");
            MathChatBotHelper.RunCommand(Properties.Resources.see_example, MathChatBotHelper.LastBotMessage);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.this_term_has_no_examples);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeDefinition()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            MathChatBotHelper.RunCommand(Properties.Resources.see_example, MathChatBotHelper.LastBotMessage);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_definition);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsTermDefinition);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeDefinitionExampleNotSelected()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_definition);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeDefinitionsSelectedTerm()
        {
            MathChatBotHelper.WriteMessageToBot("probability");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.term);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsTermDefinition);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeDefinitionsSelectedTopic()
        {
            MathChatBotHelper.WriteMessageToBot("probability");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.topic);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsTopicDefinition);
        }
        [TestMethod]
        public void MathChatBotHelper_DidNotHelp()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "akeem");
                MathChatBotHelper.User = user;
                MathChatBotHelper.WriteMessageToBot("function");
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.did_not_help_command);
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.help_request_has_been_sent_to_your_teacher);
            }
        }
        [TestMethod]
        public void MathChatBotHelper_DidNotHelpNotStudent()
        {
            using (Entity)
            {
                var user = Entity.Users.FirstOrDefault(x => x.FirstName.ToLower() == "jakob");
                MathChatBotHelper.User = user;
                MathChatBotHelper.WriteMessageToBot("function");
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.did_not_help_command);
                MathChatBotHelper.User = null;
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text != Properties.Resources.help_request_has_been_sent_to_your_teacher);
            }
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAssignmentsNoTermDefinitionExampleOrTopicDefinition()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.please_select_a_term_or_example_first_before_using_this_command);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAssignmentsForTopic()
        {
            MathChatBotHelper.WriteMessageToBot("trigonometry");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsAssignment);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAssignments()
        {
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsAssignment);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAssignmentsNoAssignments()
        {
            MathChatBotHelper.WriteMessageToBot("numbers");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.there_are_no_assignments_for_this);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeCurrentAssignment()
        {
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.clear);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.current);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsAssignment);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeCurrentAssignmentNoAssignmentsSelected()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.current);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeNextAssignment()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.next);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsAssignment);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeNextAssignmentNoMoreAssignments()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.next);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.next);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.there_are_no_more_assignments);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeNextAssignmentNoAssignmentsSelected()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.next);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_SeePreviousAssignment()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.next);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.previous);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsAssignment);
        }
        [TestMethod]
        public void MathChatBotHelper_SeePreviousAssignmentNoPreviousAssignments()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.previous);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.there_are_no_previous_assignments);
        }
        [TestMethod]
        public void MathChatBotHelper_SeePreviousAssignmentNoAssignmentsSelected()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.previous);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAnswers()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
            var assignment = MathChatBotHelper.LastBotMessage.Assignment;
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_answers);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text.Contains(assignment.AnswerA));
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAnswersNoAssignmentSelected()
        {
            MathChatBotHelper.Reset();
            MathChatBotHelper.WriteMessageToBot("sine");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_answers);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.please_select_an_assignment_first_before_using_this_command);
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAnswersSeveralAnswers()
        {
            using (Entity)
            {
                var name = Entity.Assignments.FirstOrDefault(x => x.AnswerB != null).Term.Name;
                MathChatBotHelper.Reset();
                MathChatBotHelper.WriteMessageToBot(name);
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_answers);
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text.Contains("a)"));
            }
        }
        [TestMethod]
        public void MathChatBotHelper_SeeAnswersNoAnswers()
        {
            using (Entity)
            {
                var assignment = Entity.Assignments.FirstOrDefault(x => x.Term.Name.ToLower() == "cosine");
                assignment.AnswerA = null;
                Entity.SaveChanges();
                MathChatBotHelper.Reset();
                MathChatBotHelper.WriteMessageToBot("cosine");
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_assignments);
                MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_answers);
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.this_assignment_has_no_answers);
            }
        }
        [TestMethod]
        public void MathChatBotHelper_GotDefinition()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsTermDefinition);
        }
        [TestMethod]
        public void MathChatBotHelper_AnalyzeTermWhichIsInTheDatabaseLongSentence()
        {
            MathChatBotHelper.WriteMessageToBot("cat, dog, cow, goat and acute triangle");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Material.Term.Name.ToLower() == "acute triangle");
        }
        [TestMethod]
        public void MathChatBotHelper_SeeExample()
        {
            MathChatBotHelper.WriteMessageToBot("function");
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.see_example);
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsExample);
        }
        [TestMethod]
        public void MathChatBotHelper_TermIsNotInTheDatabase()
        {
            MathChatBotHelper.WriteMessageToBot("bird");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_TermsIsNotInTheDatabase()
        {
            MathChatBotHelper.WriteMessageToBot("bird dog");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_TwoCommas()
        {
            MathChatBotHelper.WriteMessageToBot("bird,,dog");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_AdjectiveNotFollowedByAdjectiveOrNoun()
        {
            MathChatBotHelper.WriteMessageToBot("cute and bird");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_NoNouns()
        {
            MathChatBotHelper.WriteMessageToBot("cute");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_Command()
        {
            MathChatBotHelper.WriteMessageToBot(Properties.Resources.clear + "....");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_MoreThanOneSentence()
        {
            MathChatBotHelper.WriteMessageToBot("Hello how are you. What is a function");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.i_cannot_process_more_than_one_sentence);
        }
        [TestMethod]
        public void MathChatBotHelper_WrongNounListRepresentation()
        {
            MathChatBotHelper.WriteMessageToBot("bird, dog");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsMessage);
        }
        [TestMethod]
        public void MathChatBotHelper_TopicTermSelection()
        {
            MathChatBotHelper.WriteMessageToBot("probability");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsSelection);
        }
        [TestMethod]
        public void MathChatBotHelper_FoundTopic()
        {
            MathChatBotHelper.WriteMessageToBot("trigonometry");
            Assert.IsTrue(MathChatBotHelper.LastBotMessage.IsTopicDefinition);
        }
        [TestMethod]
        public void MathChatBotHelper_NoMaterials()
        {
            using (Entity)
            {
                var topic = Entity.Materials.FirstOrDefault(x => x.Topic.Name.ToLower() == "trigonometry");
                Entity.Entry(topic).State = System.Data.Entity.EntityState.Deleted;
                Entity.SaveChanges();
                MathChatBotHelper.WriteMessageToBot("trigonometry");
                Assert.IsTrue(MathChatBotHelper.LastBotMessage.Text == Properties.Resources.no_materials_found);
            }
        }
        [TestMethod]
        public void MathChatBotHelper_NullText()
        {
            try
            {
                MathChatBotHelper.WriteMessageToBot(null);
                Assert.IsTrue(true);
            }
            catch { }
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_AddQuestion()
        {
            MathChatBotHelper.WriteMessageToBot("What is 4 + 7");
            Assert.AreEqual("11", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_Add()
        {
            MathChatBotHelper.WriteMessageToBot("=0");
            MathChatBotHelper.WriteMessageToBot("5");
            MathChatBotHelper.WriteMessageToBot("8");
            MathChatBotHelper.WriteMessageToBot("11");
            Assert.AreEqual("24", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_Substraction()
        {
            MathChatBotHelper.WriteMessageToBot("=12-89");
            Assert.AreEqual("-77", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_Division()
        {
            MathChatBotHelper.WriteMessageToBot("=1024/8");
            Assert.AreEqual("128", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_Multiply()
        {
            MathChatBotHelper.WriteMessageToBot("=35*6");
            Assert.AreEqual("210", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_MultipleOperations()
        {
            MathChatBotHelper.WriteMessageToBot("=(35*6/10+6)-60");
            Assert.AreEqual("-33", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_SquareRoot()
        {
            MathChatBotHelper.WriteMessageToBot("=sqrt(121)");
            Assert.AreEqual("11", MathChatBotHelper.LastBotMessage.Text);
        }
        [TestMethod]
        public void MathChatBotHelper_SimpleCalculator_Value()
        {
            MathChatBotHelper.WriteMessageToBot("=6");
            MathChatBotHelper.WriteMessageToBot("=value");
            Assert.AreEqual("6", MathChatBotHelper.LastBotMessage.Text);
        }

        #endregion

        //*************************************************/
        // SIMPLECALCULATOR HELPER
        //*************************************************/
        #region SimpleCalculatorHelper

        [TestMethod]
        public void SimpleCalculatorHelper_ClearResult()
        {
            SimpleCalculatorHelper.UseCalculator("=4 + 8");
            var response = SimpleCalculatorHelper.UseCalculator(Properties.Resources.clear_result);
            Assert.IsTrue(response == Properties.Resources.your_total_value_has_been_set_to_0);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_NoNumber()
        {
            var response = SimpleCalculatorHelper.UseCalculator(Properties.Resources.see);
            Assert.IsTrue(response == null);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Value()
        {
            SimpleCalculatorHelper.UseCalculator("=1 + 5");
            var response = SimpleCalculatorHelper.UseCalculator("=" + Properties.Resources.value + " + 5");
            Assert.IsTrue(response == "11");
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Value_Alone()
        {
            SimpleCalculatorHelper.UseCalculator("=16");
            var response = SimpleCalculatorHelper.UseCalculator("/4");
            Assert.IsTrue(response == "4");
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Value_NoNumbers()
        {
            SimpleCalculatorHelper.UseCalculator("=1 + 15");
            var response = SimpleCalculatorHelper.UseCalculator("=sqrt(" + Properties.Resources.value + ")");
            Assert.IsTrue(response == "4");
        }
        [TestMethod]
        public void SimpleCalculatorHelper_ArgumentException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("random(9,0)");
            Assert.IsTrue(response == Properties.Resources.please_enter_proper_math_function);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_DivideByZeroException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=1/0");
            Assert.IsTrue(response == Properties.Resources.you_cannot_divide_by_zero);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_NegativeSqrtException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=sqrt(-8)");
            Assert.IsTrue(response == Properties.Resources.you_cannot_take_square_root_of_negative_number);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_AcosWrongValueException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=acos(80)");
            Assert.IsTrue(response == Properties.Resources.acos_can_only_contain_numbers_between_minus1_and_1);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_AsinWrongValueException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=asin(80)");
            Assert.IsTrue(response == Properties.Resources.asin_can_only_contain_numbers_between_minus1_and_1);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_EvaluationException()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=atan(8000000");
            Assert.IsTrue(response == Properties.Resources.please_enter_valid_input_in_functions);
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Cos()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=cos(90)");
            Assert.IsTrue(response == "0");
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Sin()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=sin(90)");
            Assert.IsTrue(response == "1");
        }
        [TestMethod]
        public void SimpleCalculatorHelper_Tan()
        {
            var response = SimpleCalculatorHelper.UseCalculator("=tan(45)");
            Assert.IsTrue(response == "1");
        }

        #endregion

    }
}
