using MathChatBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathChatBot.Utilities
{
    public class DatabaseResponse
    {
        public bool Success { get; set; }
        public User User { get; set; }

        public DatabaseResponse(bool success, User user)
        {
            Success = success;
            User = user;
        }
    }

    public static class DatabaseUtility
    {
        #region Constants

        public static string PassPhrase = "MathChatBot";

        private static MathChatBotEntities _mathChatBotEntities;
        public static MathChatBotEntities GetEntity()
        {
            if (_mathChatBotEntities == null)
                _mathChatBotEntities = new MathChatBotEntities();

            return _mathChatBotEntities;
        }

        #endregion

        #region User functions

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
                number = random.Next(1, 100);
                username = firstName.ToLower().Substring(0, 2) + lastName.ToLower().Substring(0, 2) + number.ToString();
            }
            while (entities.Users.Any(x => x.Username == username));

            return username;
        }

        public static User GetUserFromUsername(this MathChatBotEntities entities, string username)
        {
            // Get user
            var user = entities.Users.Where(x => x.Username == username).FirstOrDefault();
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
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // User not found
            if (user == null)
                return new DatabaseResponse(false, null);

            var decryptedPassword = EncryptUtility.Decrypt(user.Password, PassPhrase);
            if (decryptedPassword != password)
                return new DatabaseResponse(false, null);

            return new DatabaseResponse(user != null, user);
        }

        /// <summary>
        /// Get roles for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>A list with the user's roles</returns>
        public static List<Role> GetUserRoles(string username)
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

            var roles = entities.UserRoleRelations.Where(x => x.UserId == user.Id).Select(x => x.Role).ToList();

            return roles;
        }

        public static List<Role> GetRolesList()
        {
            var entities = GetEntity();
            return entities.Roles.ToList();
        }

        public static List<Role> GetRolesListForUser(string username)
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

            var roles = GetUserRoles(username);

            var allRoles = entities.Roles.ToList();
            foreach (var role in allRoles)
            {
                role.IsAssigned = roles.Any(x => x.Name == role.Name)
                    ? true
                    : false;
            }

            return allRoles;
        }

        public static User CreateUser(User user)
        {
            var entities = GetEntity();

            User tempUser = entities.Users.Where(x => x.Username == user.Username).FirstOrDefault();

            // Check if user exist in the database
            if (tempUser != null)
                return tempUser;

            // Create a new user and add it to the database
            var result = entities.Users.Add(new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Password = EncryptUtility.Encrypt(user.Password, PassPhrase),
                IsActivated = true
            });

            // Check if everything went ok
            if (!(result is User))
                return null;

            // Save changes
            var flag = entities.SaveChanges();
            // If the flag is 1 return true
            if (flag == 1)
                return result;

            return null;
        }

        public static bool AddToClassesByName(User user, string[] classes)
        {
            return AddToClassesByName(user.Username, classes);
        }

        public static bool AddToClassesByName(string username, string[] classes)
        {
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var clas in classes)
            {
                var databaseClass = entities.Classes.Where(x => x.Name == clas).FirstOrDefault();

                if (databaseClass == null)
                {
                    databaseClass = entities.Classes.Add(new Class()
                    {
                        Name = clas
                    });

                    var flag = entities.SaveChanges();
                    if (flag != 1)
                        return false;
                }

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

        public static bool AddRolesByName(User user, string[] roles)
        {
            return AddRolesByName(user.Username, roles);
        }

        public static bool AddRolesByName(string username, string[] roles)
        {
            var entities = GetEntity();

            // Get user
            var user = entities.GetUserFromUsername(username);

            // If the user is null return false
            if (user == null)
                return false;

            foreach (var role in roles)
            {
                var databaseRole = entities.Roles.Where(x => x.Name == role).FirstOrDefault();

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

        #endregion

    }
}
