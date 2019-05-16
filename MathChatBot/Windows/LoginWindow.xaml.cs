using MathChatBot.Models;
using MathChatBot.Utilities;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        public User User { get; private set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public LoginWindow()
        {
            InitializeComponent();

            DatabaseUtility.DisposeEntity();

            // Get saved login information
            GetSavedLogin();

            // Click events
            btnLogin.Click += button_Click;
            // KeyDown events
            tbPassword.KeyDown += textBox_KeyDown;
            tbUsername.KeyDown += textBox_KeyDown;

            // Set border
            this.SetupBorderHeader(Properties.Resources.login);
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

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
            CustomDialog.ShowProgress(Properties.Resources.logging_in_please_wait, hideCancelButton: true);
            bool saveCredentials = cbSaveCredentials.IsChecked.Value;

            this.StartThread(() =>
            {
                try
                {
                    var response = DatabaseUtility.CheckLogin(username, password);
                    if (response.Success)
                    {
                        if (saveCredentials)
                            // Save the login credentials for later use
                            SettingsUtility.SaveLoginCredentials(username, password);
                        else
                            SettingsUtility.SaveLoginCredentials();

                        this.RunOnUIThread(() =>
                        {
                            DialogResult = true;
                            User = (User)response.Data;
                            Close();
                        });
                    }
                    else
                        CustomDialog.Show(response.ErrorMessage);
                }
                catch (Exception mes)
                {
                    // Show error to the user
                    CustomDialog.Show(mes.Message);
                }
            });
        }

        #endregion

        //*************************************************/
        // EVENTS
        //*************************************************/
        #region Events

        // Button - Click
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

        // PasswordBox, TextBox - KeyDown
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(tbPassword):
                case nameof(tbUsername):
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
