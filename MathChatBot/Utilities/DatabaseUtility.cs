using MathChatBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static MathChatBot.Models.Role;

namespace MathChatBot.Utilities
{
    /// <summary>
    /// Response for when checking login
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public User User { get; set; }

        public LoginResponse(bool success, User user)
        {
            Success = success;
            User = user;
        }
    }

    public static class DatabaseUtility
    {

        //*************************************************/
        // VARIABLES
        //*************************************************/
        #region Variables

        private static MathChatBotEntities _mathChatBotEntities;
        private const string PassPhrase = "MathChatBot";

        public static void RefreshEntity()
        {
            if (_mathChatBotEntities != null)
            {
                _mathChatBotEntities.Dispose();
                _mathChatBotEntities = null;
            }
        }

        public static MathChatBotEntities Entity
        {
            get
            {
                if (_mathChatBotEntities == null)
                    _mathChatBotEntities = new MathChatBotEntities();

                return _mathChatBotEntities;
            }
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Generate username for a new user
        /// </summary>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <returns></returns>
        public static string GenerateUsername(string firstName, string lastName)
        {
            if (firstName.Length < 2 || lastName.Length < 2)
                return null;

            Random random = new Random();
            var number = 0;
            var username = string.Empty;

            do
            {
                // Generate number between 0 and 100
                number = random.Next(1, 100);
                // Generate username
                username = firstName.ToLower().Substring(0, 2) + lastName.ToLower().Substring(0, 2) + number.ToString();
            }
            // Retry if the username already exists
            while (Entity.Users.Any(x => x.Username == username));

            return username;
        }

        /// <summary>
        /// Get user from username
        /// </summary>
        /// <param name="entities">The MathChatBot entity</param>
        /// <param name="username">The user's username</param>
        /// <returns>Corresponding user for the username</returns>
        public static User GetUserFromUsername(this MathChatBotEntities entities, string username)
        {
            // Get user
            var user = entities.Users.FirstOrDefault(x => x.Username == username);
            return user;
        }

        /// <summary>
        /// Check login for an user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <returns>A response with a success flag and the user object</returns>
        public static LoginResponse CheckLogin(string username, string password)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // User not found
            if (user == null)
                return new LoginResponse(false, null);

            if (!user.IsActivated)
                return new LoginResponse(false, user);

            var decryptedPassword = EncryptUtility.Decrypt(user.Password, PassPhrase);
            if (decryptedPassword != password)
                return new LoginResponse(false, null);

            return new LoginResponse(user != null, user);
        }

        /// <summary>
        /// Get roles for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>A list with the user's roles</returns>
        public static List<RoleTypes> GetUserRoles(string username)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return null;

            var roles = user.UserRoleRelations.Select(x => x.Role.RoleType).ToList();

            return roles;
        }

        /// <summary>
        /// Get a list of all the roles in the database
        /// </summary> 
        public static List<Role> GetRolesList()
        {
            return Entity.Roles.ToList();
        }

        /// <summary>
        /// Get a list of all roles in database with an IsAssigned flag 
        /// set for each role that the user is assigned
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns></returns>
        public static List<Role> GetRolesListForUser(string username)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // If the user is null return null
            if (user == null)
                return null;

            // Get all the roles that the user is assigned
            var roles = GetUserRoles(username);
            // Get all the roles in the database
            var allRoles = Entity.Roles.ToList();

            // Set a flag for each of the roles that the user is assigned
            foreach (var role in allRoles)
            {
                role.IsAssigned = roles.Any(x => x.ToString() == role.Name)
                    ? true
                    : false;
            }

            return allRoles;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user">The user object</param>
        /// <returns>The user </returns>
        public static User CreateUser(User user)
        {
            // Get user from database
            User tempUser = Entity.GetUserFromUsername(user.Username);

            // Return the current user if it already exists
            if (tempUser != null)
                return tempUser;

            // Create a new user and add it to the database
            tempUser = Entity.Users.Add(new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Password = EncryptUtility.Encrypt(user.Password, PassPhrase),
                IsActivated = true
            });

            // Check if everything went ok
            if (!(tempUser is User))
                return null;

            // Save changes
            var flag = Entity.SaveChanges();
            // If the flag is 1 return true
            if (flag == 1)
                return tempUser;

            return null;
        }

        /// <summary>
        /// Add user to given classes
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="classes">An array with the class names</param>
        /// <returns></returns>
        public static bool AddToClassesByName(User user, string[] classes)
        {
            return AddToClassesByName(user.Username, classes);
        }

        /// <summary>
        /// Add user to given classes
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="classes">An array with the class names</param>
        /// <returns></returns>
        public static bool AddToClassesByName(string username, string[] classes)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var @class in classes)
            {
                var databaseClass = Entity.Classes.FirstOrDefault(x => x.Name == @class);

                if (databaseClass == null)
                {
                    databaseClass = Entity.Classes.Add(new Class()
                    {
                        Name = @class
                    });

                    var flag = Entity.SaveChanges();
                    if (flag != 1)
                        return false;
                }

                // Check if the class exist in the database and 
                // the relation between the class and the user has not been made
                if (databaseClass != null && !Entity.UserClassRelations.Any(x => x.UserId == user.Id && x.ClassId == databaseClass.Id))
                {
                    Entity.UserClassRelations.Add(new UserClassRelation()
                    {
                        UserId = user.Id,
                        ClassId = databaseClass.Id
                    });
                }
            }

            Entity.SaveChanges();

            return true;
        }

        /// <summary>
        /// Assign roles by name for the user
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="roles">An array with role names</param>
        /// <returns>A success flag</returns>
        public static bool AssignRolesByName(User user, string[] roles)
        {
            return AssignRolesByName(user.Username, roles);
        }

        /// <summary>
        /// Assign roles by name for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="roles">An array with role names</param>
        /// <returns></returns>
        public static bool AssignRolesByName(string username, string[] roles)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var role in roles)
            {
                // Get role from database
                var databaseRole = Entity.Roles.FirstOrDefault(x => x.Name == role);

                // Check the role exists and the relation has not been made yet
                if (databaseRole != null && !Entity.UserRoleRelations.Any(x => x.UserId == user.Id && x.RoleId == databaseRole.Id))
                {
                    Entity.UserRoleRelations.Add(new UserRoleRelation()
                    {
                        UserId = user.Id,
                        RoleId = databaseRole.Id
                    });
                }
            }

            Entity.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reset a user's password to new password
        /// </summary>
        /// <param name="newPassword">The user's new password</param>
        /// <param name="username">The user's username</param>
        /// <returns></returns>
        public static bool ResetUserPassword(string newPassword, string username)
        {
            // Get the user
            var user = Entity.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            // Set the new password
            user.Password = EncryptUtility.Encrypt(newPassword, PassPhrase);

            // Save the changes
            var flag = Entity.SaveChanges();

            // If the flag is 1 return true
            if (flag == 1)
                return true;

            return false;
        }

        /// <summary>
        /// Get all the term names under the specified topic
        /// </summary>
        /// <param name="topicName">The name of the topic</param>
        /// <returns>A list with all the term names</returns>
        public static List<string> GetTermNames(string topicName)
        {
            return Entity.Terms
                .Where(x => x.Topic.Name == topicName)
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Get all the topic names in the database
        /// </summary>
        /// <returns>A list with all the topic names</returns>
        public static List<string> GetTopicNames()
        {
            return Entity.Topics
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Get all users in a class with the specified role
        /// </summary>
        /// <param name="class">The class object</param>
        /// <param name="roleType">The specified role</param>
        /// <returns>A list with the users</returns>
        public static List<User> GetUsersInClass(Class @class, RoleTypes[] roleTypes, bool setStringRoles)
        {
            PerformanceTester.StartMET("GetUsersInClass");

            List<User> usersInClass = new List<User>();
            // Role relations
            var roleRelations = Entity.UserRoleRelations.ToList();

            if (roleTypes != null)
            {
                var roles = roleTypes.Select(x => x.GetName()).ToList();
                usersInClass = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id) && x.UserRoleRelations.Any(x2 => roles.Contains(x2.Role.Name)))
                    .ToList();
            }
            else
            {
                usersInClass = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id))
                    .ToList();
            }

            // Set string roles
            if (setStringRoles)
                usersInClass.ForEach(x => x.SetStringRole(roleRelations.Where(x2 => x2.UserId == x.Id).ToList()));

            PerformanceTester.StopMET("GetUsersInClass");

            return usersInClass.OrderBy(x => x.Name).ToList();
        }

        public static List<User> GetUsersWithHelpRequests(Class @class, string topicName = null)
        {
            PerformanceTester.StartMET("GetUsersWithHelpRequests");

            List<User> users = null;
            if (string.IsNullOrEmpty(topicName))
            {
                users = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id) && x.HelpRequests.Any())
                    .ToList()
                    .OrderBy(x => x.Name)
                    .ToList();
            }
            else
            {
                users = Entity.Users
                    .Where(x => x.UserClassRelations.Any(x2 => x2.ClassId == @class.Id) && x.HelpRequests.Any(x2 => x2.Term.Topic.Name == topicName))
                    .ToList()
                    .OrderBy(x => x.Name)
                    .ToList();
            }

            PerformanceTester.StopMET("GetUsersWithHelpRequests");

            return users;
        }

        public static List<IGrouping<Topic, HelpRequest>> GetHelpRequestsFromUsers(List<User> users, string topicName = null)
        {
            PerformanceTester.StartMET("GetHelpRequestsFromUsers");

            var ids = users.Select(x2 => x2.Id).ToList();
            var groups = Entity.HelpRequests.Where(x => ids.Contains(x.UserId) && (topicName == null || (x.Term.Topic.Name == topicName))).GroupBy(x => x.Term.Topic).ToList();

            PerformanceTester.StopMET("GetHelpRequestsFromUsers");

            return groups;
        }

        public static List<IGrouping<Topic, HelpRequest>> GetHelpRequestsFromUser(User user, string topicName = null)
        {
            PerformanceTester.StartMET("GetHelpRequestsFromUser");

            var groups = user.HelpRequests.Where(x => topicName == null || (x.Term.Topic.Name == topicName)).GroupBy(x => x.Term.Topic).ToList();

            PerformanceTester.StopMET("GetHelpRequestsFromUser");

            return groups;
        }

        /// <summary>
        /// Get users not in class
        /// </summary>
        /// <param name="class">The class object</param>
        /// <returns>A list of users</returns>
        public static List<User> GetUsersNotInClass(Class @class)
        {
            PerformanceTester.StartMET("GetUsersNotInClass");

            var studentRole = RoleTypes.Student.GetName();
            var teacherRole = RoleTypes.Teacher.GetName();

            var users = Entity.Users.Where(x => x.UserClassRelations.All(x2 => x2.ClassId != @class.Id) &&
                ((!x.UserClassRelations.Any() && x.UserRoleRelations.Any(x2 => x2.Role.Name == studentRole)) ||
                x.UserRoleRelations.Any(x2 => x2.Role.Name == teacherRole))
            ).ToList();

            // Role relations
            var roleRelations = Entity.UserRoleRelations.ToList();
            // Set string roles
            users.ForEach(x => x.SetStringRole(roleRelations.Where(x2 => x2.UserId == x.Id).ToList()));

            PerformanceTester.StopMET("GetUsersNotInClass");

            return users;
        }

        /// <summary>
        /// Set string roles for all the users
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="userRoleRelations">The users relations</param>
        public static void SetStringRole(this User user, List<UserRoleRelation> userRoleRelations)
        {
            var roles = userRoleRelations.Select(x => x.Role).ToList();
            var names = roles.OrderBy(x => x.Name).Select(x => x.Name).ToList();
            var strRoles = string.Join(", ", names);
            user.Roles = strRoles;
        }

        /// <summary>
        /// Get information about terms related to a given topic
        /// </summary>
        /// <param name="topicName">Name of the topic</param>
        /// <returns>A list of terms</returns>
        public static List<Term> GetTermInformations(string topicName)
        {
            PerformanceTester.StartMET("GetTermInformations");

            var terms = Entity.Terms
                .Where(x => x.Topic.Name == topicName)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Topic = x.Topic,
                    Assignments = (List<Assignment>)x.Assignments,
                    Materials = (List<Material>)x.Materials,
                    TopicId = x.TopicId,
                    HelpRequests = (List<HelpRequest>)x.HelpRequests,
                    MaterialsCount = (int?)x.Materials.Count ?? 0,
                    ExamplesCount = (int?)x.Materials.Sum(x2 => x2.MaterialExamples.Count) ?? 0,
                    AssignmentsCount = (int?)x.Assignments.Count ?? 0
                });
            var list = terms.AsEnumerable().Select(x =>
                 new Term
                 {
                     Id = x.Id,
                     Name = x.Name,
                     Topic = x.Topic,
                     Assignments = x.Assignments,
                     Materials = x.Materials,
                     TopicId = x.TopicId,
                     HelpRequests = x.HelpRequests,
                     MaterialsCount = x.MaterialsCount,
                     ExamplesCount = x.ExamplesCount,
                     AssignmentsCount = x.AssignmentsCount
                 }).ToList();

            PerformanceTester.StopMET("GetTermInformations");

            return list;
        }

        /// <summary>
        /// Add new topic to the database
        /// </summary>
        /// <param name="topicName">Name for the topic</param>
        /// <returns>True if the topic is already in the database or it has been added else false</returns>
        public static bool AddNewTopic(string topicName)
        {
            // Give a valid topic name
            if (string.IsNullOrEmpty(topicName))
                return false;

            // Is the topic already in the database
            if (Entity.Topics.Any(x => x.Name == topicName))
                return true;

            var topic = new Topic()
            {
                Name = topicName
            };

            Entity.Topics.Add(topic);
            var flag = Entity.SaveChanges();
            return flag == 1;
        }

        /// <summary>
        /// Add new term to the database
        /// </summary>
        /// <param name="topic">The topic object</param>
        /// <param name="termName">The name for the term</param>
        /// <returns></returns>
        public static bool AddNewTerm(Topic topic, string termName)
        {
            // Give a valid topic and term name
            if (topic == null || string.IsNullOrEmpty(termName))
                return false;

            // Is the term already in the database for the selected topic
            if (Entity.Terms.Any(x => x.Name == termName && x.Topic.Name == topic.Name))
                return true;

            var term = new Term()
            {
                Name = termName,
                Topic = topic
            };

            Entity.Terms.Add(term);
            var flag = Entity.SaveChanges();
            return flag == 1;
        }

        /// <summary>
        /// Delete a topic from the database
        /// </summary>
        /// <param name="topic">The topic object</param>
        /// <returns>True if topic was deleted else false</returns>
        public static bool DeleteTopic(Topic topic)
        {
            if (topic == null)
                return false;

            if (topic.Terms.Any() ||
                topic.Materials.Any())
                return false;

            Entity.Entry(topic).State = System.Data.Entity.EntityState.Deleted;

            try
            {
                return Entity.SaveChanges() == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete term from the database
        /// </summary>
        /// <param name="term">The term object</param>
        /// <returns>True if term was deleted else false</returns>
        public static bool DeleteTerm(Term term)
        {
            if (term == null)
                return false;

            if (term.HelpRequests.Any() ||
                term.Materials.Any() ||
                term.Assignments.Any())
                return false;

            Entity.Entry(term).State = System.Data.Entity.EntityState.Deleted;

            try
            {
                return Entity.SaveChanges() == 1;
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }
}
