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

        public static MathChatBotEntities GetEntity()
        {
            if (_mathChatBotEntities == null)
                _mathChatBotEntities = new MathChatBotEntities();

            return _mathChatBotEntities;
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

            var entities = GetEntity();

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
            while (entities.Users.Any(x => x.Username == username));

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
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // User not found
            if (user == null)
                return new LoginResponse(false, null);

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
            var entities = GetEntity();

            // Check if user exist in the database
            if (!entities.Users.Any(x => x.Username == username))
                return null;

            // Get user
            var user = entities.Users.Where(x => x.Username == username).FirstOrDefault();

            // If the user is null return false
            if (user == null)
                return null;

            var roles = user.UserRoleRelations.Select(x => x.Role).Select(x => x.RoleType).ToList();

            return roles;
        }

        /// <summary>
        /// Get a list of all the roles in the database
        /// </summary> 
        public static List<Role> GetRolesList()
        {
            var entities = GetEntity();
            return entities.Roles.ToList();
        }

        /// <summary>
        /// Get a list of all roles in database with an IsAssigned flag 
        /// set for each role that the user is assigned
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns></returns>
        public static List<Role> GetRolesListForUser(string username)
        {
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return null
            if (user == null)
                return null;

            // Get all the roles that the user is assigned
            var roles = GetUserRoles(username);
            // Get all the roles in the database
            var allRoles = entities.Roles.ToList();

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
            var entities = GetEntity();

            // Get user from database
            User tempUser = entities.GetUserFromUsername(user.Username);

            // Return the current user if it already exists
            if (tempUser != null)
                return tempUser;

            // Create a new user and add it to the database
            tempUser = entities.Users.Add(new User()
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
            var flag = entities.SaveChanges();
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
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var @class in classes)
            {
                var databaseClass = entities.Classes.FirstOrDefault(x => x.Name == @class);

                if (databaseClass == null)
                {
                    databaseClass = entities.Classes.Add(new Class()
                    {
                        Name = @class
                    });

                    var flag = entities.SaveChanges();
                    if (flag != 1)
                        return false;
                }

                // Check if the class exist in the database and 
                // the relation between the class and the user has not been made
                if (databaseClass != null && !entities.UserClassRelations.Any(x => x.UserId == user.Id && x.ClassId == databaseClass.Id))
                {
                    entities.UserClassRelations.Add(new UserClassRelation()
                    {
                        UserId = user.Id,
                        ClassId = databaseClass.Id
                    });
                }
            }

            entities.SaveChanges();

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
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var role in roles)
            {
                // Get role from database
                var databaseRole = entities.Roles.FirstOrDefault(x => x.Name == role);

                // Check the role exists and the relation has not been made yet
                if (databaseRole != null && !entities.UserRoleRelations.Any(x => x.UserId == user.Id && x.RoleId == databaseRole.Id))
                {
                    entities.UserRoleRelations.Add(new UserRoleRelation()
                    {
                        UserId = user.Id,
                        RoleId = databaseRole.Id
                    });
                }
            }

            entities.SaveChanges();

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
            var entities = GetEntity();

            // Get the user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            // Set the new password
            user.Password = EncryptUtility.Encrypt(newPassword, PassPhrase);

            // Save the changes
            var flag = entities.SaveChanges();

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
            return GetEntity().Terms
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
            return GetEntity().Topics
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Get all users in a class with the specified role
        /// </summary>
        /// <param name="class">The class object</param>
        /// <param name="roleType">The specified role</param>
        /// <returns>A list with the users</returns>
        public static List<User> GetUsersInClass(Class @class, RoleTypes roleType)
        {
            return @class.UserClassRelations
                .Select(x => x.User)
                .Where(x => x.UserRoleRelations.Any(x2 => x2.Role.RoleType == roleType))
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Set get roles in a list representation of nouns
        /// </summary>
        /// <param name="list">The list of users</param>
        public static void SetStringRolesForUsers(this List<User> list)
        {
            foreach (var user in list)
            {
                var roles = user.UserRoleRelations.Select(x => x.Role).ToList();

                if (roles == null)
                {
                    user.Roles = "";
                    continue;
                }

                var names = roles.OrderBy(x => x.Name).Select(x => x.Name).ToList();

                var strRoles = string.Join(", ", names);
                user.Roles = strRoles;
            }
        }

        #endregion

    }
}
