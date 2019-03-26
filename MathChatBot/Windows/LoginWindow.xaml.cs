using MathChatBot.Models;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Constructor

        public LoginWindow()
        {
            InitializeComponent();

            GetSavedLogin();

            // Click events
            btnLogin.Click += button_Click;
            // KeyDown events
            tbPassword.KeyDown += textBox_KeyDown;

            this.SetupBorderHeader(Properties.Resources.login);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Get saved login from settings
        /// </summary>
        private void GetSavedLogin()
        {
            var username = Properties.Settings.Default.Username;
            var password = Properties.Settings.Default.Password;
            
            tbUsername.Text = username;
            tbPassword.Password = password;
            

            if (username != string.Empty && password != string.Empty)
            {
                cbSaveCredentials.IsChecked = true;
                btnLogin.Focus();
            }
        }

        /// <summary>
        /// Check login in the LoginWindow
        /// </summary>
        /// <param name="username">The given username</param>
        /// <param name="password">The given password</param>
        private void CheckLogin(string username, string password)
        {
            var response = DatabaseUtility.CheckLogin(username, password);
            if (response.Success)
            {
                if (cbSaveCredentials.IsChecked.Value)                
                    // Save the login credentials for later use
                    SettingsUtility.SaveLoginCredentials(username, password);
                else
                    SettingsUtility.SaveLoginCredentials(null, null);
                // Open the main window
                MainWindow mainWindow = new MainWindow((User)response.Object);
                mainWindow.Show();
                // Close this window
                Close();
            }
        }

        #endregion

        #region Events

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                case nameof(btnLogin):
                    {
                        CheckLogin(tbUsername.Text, tbPassword.Password);
                        break;
                    }
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            var name = "";

            if (sender is TextBox)
                name = ((TextBox)sender).Name;
            else if (sender is PasswordBox)
                name = ((PasswordBox)sender).Name;

            switch (name)
            {
                case nameof(tbPassword):
                    {
                        if (e.Key == Key.Enter)
                            CheckLogin(tbUsername.Text, tbPassword.Password);
                        break;
                    }
            }
        }

        #endregion


    }
}
