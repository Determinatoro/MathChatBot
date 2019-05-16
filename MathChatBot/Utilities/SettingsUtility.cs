using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathChatBot.Utilities
{
    public class SettingsUtility
    {

        /// <summary>
        /// Save login crendentials for the user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        public static void SaveLoginCredentials(string username = null, string password = null)
        {
            Properties.Settings.Default.Username = username;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.Save();
        }
    }
}
