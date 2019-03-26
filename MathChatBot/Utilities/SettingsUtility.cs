using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathChatBot.Utilities
{
    public class SettingsUtility
    {
        public static void SaveLoginCredentials(string username, string password)
        {
            Properties.Settings.Default.Username = username;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.Save();
        }
    }
}
