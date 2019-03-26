using MathChatBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace MathChatBot.Utilities
{
    public class DatabaseResponse
    {
        public bool Success { get; set; }
        public object Object { get; set; }

        public DatabaseResponse(bool success, object obj)
        {
            Success = success;
            Object = obj;
        }
    }

    public class DatabaseUtility
    {
        #region Constants

        public static string PassPhrase = "MathChatBot";

        #endregion

        #region User functions              

        /// <summary>
        /// Check login for an user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <returns>A response with a success flag and the user object</returns>
        public static DatabaseResponse CheckLogin(string username, string password)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
                // Check if user exist in the database
                if (!entities.Users.Any(x => x.Username == username))
                    return new DatabaseResponse(false, null);

                var user = entities.Users.Where(x => x.Username == username).FirstOrDefault();
                if (user == null)
                    return new DatabaseResponse(false, null);

                var decryptedPassword = EncryptUtility.Decrypt(user.Password, PassPhrase);
                if (decryptedPassword != password)
                    return new DatabaseResponse(false, null);

                return new DatabaseResponse(user != null, user);
            }
        }

        /// <summary>
        /// Get roles for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>A list with the user's roles</returns>
        public static List<Role> GetUserRoles(string username)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
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
        }

        public static List<Role> GetRolesListForUser(string username)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
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
        }

        public static User CreateUser(User user)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
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
                    Password = EncryptUtility.Encrypt(user.Password, PassPhrase)
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
        }

        public static bool AddRolesByName(User user, string[] roles)
        {
            return AddRolesByName(user.Username, roles);
        }

        public static bool AddRolesByName(string username, string[] roles)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
                // Check if user exist in the database
                if (!entities.Users.Any(x => x.Username == username))
                    return false;

                // Get user
                var user = entities.Users.Where(x => x.Username == username).FirstOrDefault();

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
        }

        /// <summary>
        /// Reset a user's password to new password
        /// </summary>
        /// <param name="newPassword">The user's new password</param>
        /// <param name="username">The user's username</param>
        /// <returns></returns>
        public static bool ResetUserPassword(string newPassword, string username)
        {
            using (MathChatBotEntities entities = new MathChatBotEntities())
            {
                // Check if user exist in the database
                if (!entities.Users.Any(x => x.Username == username))
                    return false;

                // Get the user
                var user = entities.Users.Where(x => x.Username == username).FirstOrDefault();

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
        }

        #endregion

    }
}
