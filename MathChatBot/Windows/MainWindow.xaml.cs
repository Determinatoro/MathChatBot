using MathChatBot.Models;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        public User User { get; set; }

        #endregion

        #region Constructor

        public MainWindow(User user) : this()
        {
            User = user;
            var roles = DatabaseUtility.GetUserRoles(user.Username);

            if (!roles.Any(x => x.RoleType == Role.RoleTypes.Administrator))
                btnAdminControls.Visibility = Visibility.Collapsed;

            //NLPUtility.ProcessText();
        }

        public MainWindow()
        {
            InitializeComponent();

            btnSend.Click += button_Click;
            btnAdminControls.Click += button_Click;
            btnLogOut.Click += button_Click;

            tbChat.KeyDown += textBox_KeyDown;
            borderHeader.MouseLeftButtonDown += BorderHeader_MouseDown;
            SetupChat();
        }

        #endregion

        #region Events

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            switch (tb.Name)
            {
                case nameof(tbChat):
                    {
                        if (e.Key == Key.Enter)
                        {
                            AddChatObject(tb.Text);
                            tb.Text = string.Empty;
                        }
                    }
                    break;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                case nameof(btnSend):
                    {


                    }
                    break;
                case nameof(btnAdminControls):
                    {
                        var adminControlsWindow = new AdminControlsWindow(User);
                        adminControlsWindow.ShowDialog();
                    }
                    break;
                case nameof(btnLogOut):
                    {
                        var loginWindow = new LoginWindow();
                        loginWindow.Show();
                        Close();
                    }
                    break;
            }
        }

        private void BorderHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #endregion

    }
}
