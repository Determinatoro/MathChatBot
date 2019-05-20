using MathChatBot.Models;
using MathChatBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static MathChatBot.Models.Role;
using Entity = MathChatBot.Models.MathChatBotEntities;

namespace MathChatBot.Utilities
{

    /// <summary>
    /// Database response
    /// </summary>
    public class DatabaseResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public object Data { get; set; }

        // Success no data
        public DatabaseResponse()
        {
            Success = true;
        }

        // Success return data
        public DatabaseResponse(object data)
        {
            Success = true;
            Data = data;
        }

        // Error
        public DatabaseResponse(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Custom entity to check if it is disposed
    /// </summary>
    public class CustomEntity : Entity
    {
        public CustomEntity()
            : base()
        {
        }

        public CustomEntity(System.Data.Common.DbConnection dbConnection, bool contextOwnsConnection)
            : base(dbConnection, contextOwnsConnection)
        {
        }

        public bool IsDisposed { get; set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Login for the DatabaseUtility
    /// </summary>
    public static class DatabaseUtility
    {
        //*************************************************/
        // VARIABLES
        //*************************************************/
        #region Variables

        private static CustomEntity _mathChatBotEntities;
        public const string PassPhrase = "MathChatBot";


        #endregion

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        public static bool TestMode { get; set; }

        public static Entity Entity
        {
            get
            {
                if (_mathChatBotEntities == null || _mathChatBotEntities.IsDisposed)
                {
                    if (TestMode)
                        _mathChatBotEntities = (CustomEntity)TestUtility.GetInMemoryContext();
                    else
                        _mathChatBotEntities = new CustomEntity();
                }

                return _mathChatBotEntities;
            }
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Dispose entity
        /// </summary>
        public static void DisposeEntity()
        {
            if (_mathChatBotEntities != null)
            {
                _mathChatBotEntities.Dispose();
                _mathChatBotEntities = null;
            }
        }

        private static DatabaseResponse RunDatabaseFunction(Func<object> func)
        {
            try
            {
                var result = func();
                if (result == null)
                    return new DatabaseResponse();
                else if (result is string)
                    return new DatabaseResponse(errorMessage: (string)result);
                else
                    return new DatabaseResponse(result);
            }
            catch (Exception mes)
            {
                return new DatabaseResponse(mes.Message);
            }
        }

        /// <summary>
        /// Get users from database
        /// </summary>
        /// <returns>A list of user objects</returns>
        public static List<User> GetUsers()
        {
            return Entity.Users
                         .Where(x => x.FirstName == "John")
                         .ToList();
        }

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
        /// <param name="entity">The MathChatBot entity</param>
        /// <param name="username">The user's username</param>
        /// <returns>Corresponding user for the username</returns>
        public static User GetUserFromUsername(this Entity entity, string username)
        {
            // Get user
            var user = entity.Users.FirstOrDefault(x => x.Username == username);
            return user;
        }

        /// <summary>
        /// Check login for an user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <returns>A response with a success flag and the user object</returns>
        public static DatabaseResponse CheckLogin(string username, string password)
        {
            // Get user
            var user = Entity.GetUserFromUsername(username);

            // User not found
            if (user == null)
                return new DatabaseResponse(Properties.Resources.wrong_username_or_password);

            var decryptedPassword = EncryptUtility.Decrypt(user.Password, PassPhrase);
            if (decryptedPassword != password)
                return new DatabaseResponse(Properties.Resources.wrong_username_or_password);

            if (!user.IsActivated)
                return new DatabaseResponse(Properties.Resources.user_is_deactivated_contact_your_administrator);

            return new DatabaseResponse(user);
        }

        /// <summary>
        /// Get roles for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>A list with the user's roles</returns>
        public static List<RoleTypes> GetUserRoles(this Entity entity, string username)
        {
            // Get user
            var user = entity.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return null;

            // Get the user's roles
            var roles = user.UserRoleRelations.Select(x => x.Role.RoleType).ToList();

            return roles;
        }

        /// <summary>
        /// Check if user is a student
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="userId">The user's ID</param>
        /// <returns></returns>
        private static bool IsStudent(this Entity entity, int userId)
        {
            var studentRole = RoleTypes.Student.GetName();
            return entity.UserRoleRelations.Any(x => x.UserId == userId && x.Role.Name == studentRole);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="rolesListForUser">The roles list for the user</param>
        public static void UpdateUserInformation(User user, List<Role> rolesListForUser)
        {
            using (var entity = Entity)
            {
                // Get user
                user = entity.Users.FirstOrDefault(x => x.Id == user.Id);
                // Get user relations
                var userRoleRelations = user.UserRoleRelations;

                foreach (var role in rolesListForUser)
                {
                    var userRoleRelation = userRoleRelations.FirstOrDefault(x => x.RoleId == role.Id);

                    if ((role.IsAssigned && userRoleRelation != null) || (!role.IsAssigned && userRoleRelation == null))
                        continue;

                    if (role.IsAssigned)
                    {
                        entity.UserRoleRelations.Add(new UserRoleRelation()
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                    }
                    else
                        entity.Entry(userRoleRelation).State = System.Data.Entity.EntityState.Deleted;
                }
                entity.SaveChanges();
            }
        }

        /// <summary>
        /// Get users ordered alphabetically
        /// </summary>
        /// <returns></returns>
        public static List<User> GetUsersOrderedAlphabetically()
        {
            return Entity.Users.ToList().OrderBy(x => x.FirstName,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ThenBy(x => x.LastName,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Get classes ordered alphabetically
        /// </summary>
        /// <returns></returns>
        public static List<Class> GetClassesOrderedAlphabetically()
        {
            return Entity.Classes.ToList().OrderBy(x => x.Name,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ToList();
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
            var roles = Entity.GetUserRoles(username);
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
        public static User CreateUser(User user, List<Role> rolesListForUser = null)
        {
            using (var entity = Entity)
            {
                // Get user from database
                User tempUser = entity.GetUserFromUsername(user.Username);

                // Return the current user if it already exists
                if (tempUser != null)
                    return tempUser;

                // Create a new user and add it to the database
                tempUser = entity.Users.Add(new User()
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
                var flag = entity.SaveChanges();

                if (rolesListForUser == null)
                    return tempUser;

                // Add roles for the user
                foreach (var role in rolesListForUser)
                {
                    if (role.IsAssigned)
                    {
                        entity.UserRoleRelations.Add(new UserRoleRelation()
                        {
                            UserId = tempUser.Id,
                            RoleId = role.Id
                        });
                    }
                }

                entity.SaveChanges();

                return tempUser;
            }
        }

        /// <summary>
        /// Add user to given classes
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="classes">An array with the class names</param>
        /// <returns></returns>
        public static bool AddToClassesByName(User user, HashSet<string> classes)
        {
            return AddToClassesByName(user.Username, classes);
        }

        /// <summary>
        /// Add user to given classes
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="classes">An array with the class names</param>
        /// <returns></returns>
        public static bool AddToClassesByName(string username, HashSet<string> classes)
        {
            using (var entity = Entity)
            {
                // Get user
                var user = entity.GetUserFromUsername(username);

                // If the user is null return false
                if (user == null)
                    return false;

                var userClassRelations = entity.UserClassRelations
                    .Where(x => x.UserId == user.Id)
                    .ToList();

                foreach (var @class in classes)
                {
                    var databaseClass = entity.Classes.FirstOrDefault(x => x.Name == @class);

                    if (databaseClass == null)
                    {
                        databaseClass = entity.Classes.Add(new Class()
                        {
                            Name = @class
                        });

                        var flag = entity.SaveChanges();
                        if (flag != 1)
                            return false;
                    }

                    // Check if the class exist in the database and 
                    // the relation between the class and the user has not been made
                    if (databaseClass != null && !entity.UserClassRelations.Any(x => x.ClassId == databaseClass.Id))
                    {
                        entity.UserClassRelations.Add(new UserClassRelation()
                        {
                            UserId = user.Id,
                            ClassId = databaseClass.Id
                        });
                    }
                }

                entity.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Assign roles by name for the user
        /// </summary>
        /// <param name="user">The user object</param>
        /// <param name="roles">An array with role names</param>
        /// <returns>A success flag</returns>
        public static bool AssignRolesByName(User user, HashSet<string> roles)
        {
            return AssignRolesByName(user.Username, roles);
        }

        /// <summary>
        /// Assign roles by name for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="roles">An array with role names</param>
        /// <returns></returns>
        public static bool AssignRolesByName(string username, HashSet<string> roles)
        {
            using (var entity = Entity)
            {
                // Get user
                var user = entity.GetUserFromUsername(username);

                // If the user is null return false
                if (user == null)
                    return false;

                var dbRoles = entity.Roles.ToList();
                var userRoleRelations = entity.UserRoleRelations
                    .Where(x => x.UserId == user.Id)
                    .ToList();

                foreach (var role in roles)
                {
                    // Get role from database
                    var databaseRole = dbRoles.FirstOrDefault(x => x.Name == role);

                    // Check the role exists and the relation has not been made yet
                    if (databaseRole != null && !userRoleRelations.Any(x => x.RoleId == databaseRole.Id))
                    {
                        entity.UserRoleRelations.Add(new UserRoleRelation()
                        {
                            UserId = user.Id,
                            RoleId = databaseRole.Id
                        });
                    }
                }

                var flag = entity.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Reset a user's password to new password
        /// </summary>
        /// <param name="newPassword">The user's new password</param>
        /// <param name="username">The user's username</param>
        /// <returns></returns>
        public static bool ResetUserPassword(ref string errorMessage, string newPassword, string username)
        {
            if (newPassword.Length < 8)
            {
                errorMessage = Properties.Resources.the_password_has_to_be_at_least_eight_characters_long;
                return false;
            }

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
        /// Get topic name from material
        /// </summary>
        /// <param name="material">Material object</param>
        /// <returns></returns>
        public static string GetTopicName(Material material)
        {
            return Entity.Materials.Find(material.Id).Topic?.Name;
        }

        /// <summary>
        /// Get term name from material
        /// </summary>
        /// <param name="material">Material object</param>
        /// <returns>Term name</returns>
        public static string GetTermName(Material material = null, MaterialExample materialExample = null, Assignment assignment = null)
        {
            if (material != null)
                return Entity.Materials.Find(material.Id).Term.Name;
            else if (materialExample != null)
                return Entity.MaterialExamples.Find(materialExample.Id).Material.Term.Name;
            else if (assignment != null)
                return Entity.Assignments.Find(assignment.Id).Term.Name;

            return string.Empty;
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
        /// Add a relation between a user and a class
        /// </summary>
        /// <param name="class">Class object</param>
        /// <param name="users">Users list</param>
        public static void AddUsersToClass(Class @class, List<User> users)
        {
            using (var entity = Entity)
            {
                foreach (var user in users)
                {
                    Entity.UserClassRelations.Add(new UserClassRelation()
                    {
                        ClassId = @class.Id,
                        UserId = user.Id
                    });
                }

                Entity.SaveChanges();
            }
        }

        /// <summary>
        /// Add class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <returns></returns>
        public static bool AddClass(ref string errorMessage, string className)
        {
            using (var entity = Entity)
            {
                if (entity.Classes.Any(x => x.Name == className))
                {
                    errorMessage = Properties.Resources.the_class_already_exists;
                    return false;
                }

                entity.Classes.Add(new Class()
                {
                    Name = className
                });

                var flag = Entity.SaveChanges();
                if (flag != 1)
                {
                    errorMessage = Properties.Resources.could_not_add_class;
                    return false;
                }

                return true;
            }
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

        /// <summary>
        /// Get users 
        /// </summary>
        /// <param name="class"></param>
        /// <param name="topicName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get help requests from the given users
        /// </summary>
        /// <param name="users">The users list</param>
        /// <param name="topicName">If specified help requests only for this topic will be retrieved from the database</param>
        /// <returns></returns>
        public static List<IGrouping<Topic, HelpRequest>> GetHelpRequestsFromUsers(List<User> users, string topicName = null)
        {
            PerformanceTester.StartMET("GetHelpRequestsFromUsers");

            var ids = users.Select(x2 => x2.Id).ToList();
            var groups = Entity.HelpRequests.Where(x => ids.Contains(x.UserId) && (topicName == null || (x.Term.Topic.Name == topicName))).GroupBy(x => x.Term.Topic).ToList();

            PerformanceTester.StopMET("GetHelpRequestsFromUsers");

            return groups;
        }

        /// <summary>
        /// Get help requests from the given users (SLOW)
        /// </summary>
        /// <param name="users">The users list</param>
        /// <param name="topicName">If specified help requests only for this topic will be retrieved from the database</param>
        /// <returns></returns>
        public static List<IGrouping<Topic, HelpRequest>> GetHelpRequestsFromUsersSlow(List<User> users, string topicName = null)
        {
            PerformanceTester.StartMET("GetHelpRequestsFromUsers");

            var ids = users.Select(x2 => x2.Id).ToList();
            var groups = users.SelectMany(x => x.HelpRequests.Where(x2 => topicName == null || (x2.Term.Topic.Name == topicName))).GroupBy(x => x.Term.Topic).ToList();

            PerformanceTester.StopMET("GetHelpRequestsFromUsers");

            return groups;
        }

        /// <summary>
        /// Get help requests for a single user
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="topicName">If specified help requests only for this topic will be retrieved from the database</param>
        /// <returns></returns>
        public static List<IGrouping<Topic, HelpRequest>> GetHelpRequestsFromUser(User user, string topicName = null)
        {
            PerformanceTester.StartMET("GetHelpRequestsFromUser");

            var groups = Entity.HelpRequests.Where(x => x.UserId == user.Id && (topicName == null || x.Term.Topic.Name == topicName)).GroupBy(x => x.Term.Topic).ToList();

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
                    x.Id,
                    x.Name,
                    x.Topic,
                    Assignments = (List<Assignment>)x.Assignments,
                    Materials = (List<Material>)x.Materials,
                    x.TopicId,
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
                 })
                 .OrderBy(x => x.Name)
                 .ToList();

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

        /// <summary>
        /// Add new topic definition
        /// </summary>
        /// <param name="topic">The topic object</param>
        /// <param name="source">The source image</param>
        public static void AddTopicMaterial(Topic topic, string source)
        {
            using (var entity = Entity)
            {
                var material = new Material()
                {
                    TopicId = topic.Id,
                    Source = source
                };
                entity.Materials.Add(material);
                entity.SaveChanges();
            }
        }

        /// <summary>
        /// Add new term material
        /// </summary>
        /// <param name="term">The term object</param>
        /// <param name="source">The source image</param>
        public static void AddTermMaterial(Term term, string source)
        {
            using (var entity = Entity)
            {
                var material = new Material()
                {
                    TermId = term.Id,
                    Source = source
                };
                entity.Materials.Add(material);
                entity.SaveChanges();
            }
        }

        /// <summary>
        /// Add new material example for a material
        /// </summary>
        /// <param name="material">The material object</param>
        /// <param name="source">The source image</param>
        public static void AddTermMaterialExample(Material material, string source)
        {
            using (var entity = Entity)
            {
                var materialExample = new MaterialExample()
                {
                    Source = source,
                    MaterialId = material.Id
                };
                Entity.MaterialExamples.Add(materialExample);
                Entity.SaveChanges();
            }
        }

        /// <summary>
        /// Add new assignment for a term
        /// </summary>
        /// <param name="term">The term object</param>
        /// <param name="source">The source image</param>
        /// <param name="answerA">Answer A for the assignment</param>
        /// <param name="answerB">Answer B for the assignment</param>
        /// <param name="answerC">Answer C for the assignment</param>
        /// <param name="answerD">Answer D for the assignment</param>
        /// <param name="answerE">Answer E for the assignment</param>
        /// <param name="answerF">Answer F for the assignment</param>
        /// <param name="answerG">Answer G for the assignment</param>
        public static DatabaseResponse AddAssignment(Term term, string source, string answerA, string answerB, string answerC, string answerD, string answerE, string answerF, string answerG)
        {
            return RunDatabaseFunction(() =>
            {
                using (var entity = Entity)
                {
                    Func<string, string> func = (s) =>
                    {
                        return string.IsNullOrEmpty(s != null ? s.Trim() : s) ? null : s;
                    };

                    answerA = func(answerA);
                    answerB = func(answerB);
                    answerC = func(answerC);
                    answerD = func(answerD);
                    answerE = func(answerE);
                    answerF = func(answerF);
                    answerG = func(answerG);

                    var assignment = new Assignment()
                    {
                        AnswerA = answerA,
                        AnswerB = answerB,
                        AnswerC = answerC,
                        AnswerD = answerD,
                        AnswerE = answerE,
                        AnswerF = answerF,
                        AnswerG = answerG,
                        Source = source,
                        TermId = term.Id
                    };

                    entity.Assignments.Add(assignment);
                    entity.SaveChanges();
                    return null;
                }
            });

        }

        /// <summary>
        /// Search for term and topic
        /// </summary>
        /// <param name="term">Term object</param>
        /// <param name="topic">Topic object</param>
        /// <param name="searchString">Search string</param>
        public static void GetTermAndTopic(out Term term, out Topic topic, string searchString)
        {
            searchString = searchString.ToLower();
            PerformanceTester.StartMET("Term");
            term = Entity.Terms.FirstOrDefault(x => x.Name.ToLower() == searchString);
            PerformanceTester.StopMET("Term");
            PerformanceTester.StartMET("Topic");
            topic = Entity.Topics.FirstOrDefault(x => x.Name.ToLower() == searchString);
            PerformanceTester.StopMET("Topic");
        }

        /// <summary>
        /// Get examples for a material
        /// </summary>
        /// <param name="materialId">Id for a material</param>
        /// <returns>A list of MaterialExample objects</returns>
        public static List<MaterialExample> GetExamples(int materialId)
        {
            PerformanceTester.StartMET("Get examples");
            var examples = Entity.MaterialExamples
                    .Where(x => x.MaterialId == materialId)
                    .OrderBy(x => x.ShowOrderId)
                    .ToList();
            PerformanceTester.StopMET("Get examples");
            return examples;
        }

        /// <summary>
        /// Get materials for a term or a topic
        /// </summary>
        /// <param name="termId">Id for a term</param>
        /// <param name="topicId">Id for a topic</param>
        /// <returns>A list of Material objects</returns>
        public static List<Material> GetMaterials(int? termId = null, int? topicId = null)
        {
            PerformanceTester.StartMET("Get materials");
            if (termId != null)
            {
                var materials = Entity.Materials
                                .Where(x => x.TermId == termId)
                                .OrderBy(x => x.ShowOrderId)
                                .ToList();
                PerformanceTester.StopMET("Get materials");
                return materials;
            }
            else if (topicId != null)
            {
                var materials = Entity.Materials
                                .Where(x => x.TopicId == topicId)
                                .OrderBy(x => x.ShowOrderId)
                                .ToList();
                PerformanceTester.StopMET("Get materials");
                return materials;
            }

            PerformanceTester.StopMET("Get materials");
            return new List<Material>();
        }

        /// <summary>
        /// Get assignments for either term or topic
        /// </summary>
        /// <param name="termId">For term</param>
        /// <param name="topicId">For topic</param>
        /// <returns></returns>
        public static List<Assignment> GetAssignments(int? termId, int? topicId = null)
        {
            if (termId != null)
            {
                return Entity.Assignments.Where(x => x.TermId == termId)
                .OrderBy(x => x.AssignmentNo)
                .ToList();
            }
            else if (topicId != null)
            {
                return Entity.Assignments
                    .Where(x => x.Term.TopicId == topicId)
                    .OrderBy(x => x.Term.Name)
                    .ToList();
            }

            return new List<Assignment>();
        }

        /// <summary>
        /// Make help request
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="termId"></param>
        /// <param name="materialId"></param>
        /// <param name="materialExampleId"></param>
        /// <param name="assignmentId"></param>
        /// <returns>A response</returns>
        public static DatabaseResponse MakeHelpRequest(int userId, int? termId, int? materialId, int? materialExampleId, int? assignmentId)
        {
            return RunDatabaseFunction(() =>
            {
                using (var entity = Entity)
                {
                    // If you are not a student you cannot make help requests
                    if (!entity.IsStudent(userId))
                        return Properties.Resources.you_need_to_be_a_student_to_make_help_requests;

                    if (termId == null)
                        return Properties.Resources.could_not_make_a_help_request;

                    // Create help request
                    var helpRequest = new HelpRequest()
                    {
                        UserId = userId,
                        TermId = termId.Value,
                        MaterialId = materialId,
                        MaterialExampleId = materialExampleId,
                        AssignmentId = assignmentId
                    };

                    // Check for existing help requests
                    if (entity.HelpRequests.Any(
                        x => x.UserId == helpRequest.UserId &&
                        x.MaterialId == materialId &&
                        x.MaterialExampleId == materialExampleId &&
                        x.AssignmentId == assignmentId)
                        )
                        return Properties.Resources.you_have_already_sent_a_help_request_for_this_material;

                    // Add help request
                    entity.Entry(helpRequest).State = System.Data.Entity.EntityState.Added;

                    if (entity.SaveChanges() == 1)
                        return null;
                    else
                        return Properties.Resources.could_not_make_a_help_request;
                }
            });

        }

        /// <summary>
        /// Get sources for the help requests
        /// </summary>
        /// <param name="termName">Name of the term</param>
        /// <returns></returns>
        public static DatabaseResponse GetHelpRequestSources(string termName)
        {
            return RunDatabaseFunction(() =>
            {
                PerformanceTester.StartMET("GetHelpRequestSources");
                // Get temp objects
                var tempObjects = Entity.HelpRequests.Where(x => x.Term.Name.ToLower() == termName.ToLower()).Select(x =>
                new
                {
                    x.AssignmentId,
                    x.MaterialId,
                    x.MaterialExampleId,
                    AssignmentSource = x.Assignment != null ? x.Assignment.Source : null,
                    MaterialSource = x.Material != null ? x.Material.Source : null,
                    ExampleSource = x.MaterialExample != null ? x.MaterialExample.Source : null,
                }).ToList();

                PerformanceTester.StopMET("GetHelpRequestSources");

                // Were there any help requests for the term
                if (!tempObjects.Any())
                    return Properties.Resources.no_help_requests_for_this_term;

                var assignments = tempObjects.Where(x => x.AssignmentId != null).GroupBy(x => x.AssignmentId);
                var materials = tempObjects.Where(x => x.MaterialId != null).GroupBy(x => x.MaterialId);
                var examples = tempObjects.Where(x => x.MaterialExampleId != null).GroupBy(x => x.MaterialExampleId);

                var sourceObjects = new List<SourceObject>();
                sourceObjects.AddRange(
                    materials
                    .Select(x => new SourceObject(x.Count(), x.FirstOrDefault()?.MaterialSource, x.FirstOrDefault()?.ExampleSource, x.FirstOrDefault()?.AssignmentSource))
                    .ToList());
                sourceObjects.AddRange(
                    examples
                    .Select(x => new SourceObject(x.Count(), x.FirstOrDefault()?.MaterialSource, x.FirstOrDefault()?.ExampleSource, x.FirstOrDefault()?.AssignmentSource))
                    .ToList());
                sourceObjects.AddRange(
                    assignments
                    .Select(x => new SourceObject(x.Count(), x.FirstOrDefault()?.MaterialSource, x.FirstOrDefault()?.ExampleSource, x.FirstOrDefault()?.AssignmentSource))
                    .ToList());

                return sourceObjects;
            });
        }

        /// <summary>
        /// Reset help requests for a class
        /// </summary>
        /// <param name="class">Class object</param>
        /// <returns>A database response</returns>
        public static DatabaseResponse ResetHelpRequests(Class @class)
        {
            try
            {
                using (var entity = Entity)
                {
                    var helpRequests = entity.HelpRequests
                        .Where(x => x.User.UserClassRelations.Any(x2 => x2.ClassId == @class.Id))
                        .ToList();

                    if (!helpRequests.Any())
                        return new DatabaseResponse();

                    helpRequests.ForEach(x => entity.Entry(x).State = System.Data.Entity.EntityState.Deleted);
                    if (Entity.SaveChanges() != helpRequests.Count)
                        return new DatabaseResponse(Properties.Resources.something_went_wrong_when_saving_changes_to_the_database);

                    return new DatabaseResponse();
                }
            }
            catch (Exception mes)
            {
                return new DatabaseResponse(mes.Message);
            }
        }

        #endregion

    }
}
