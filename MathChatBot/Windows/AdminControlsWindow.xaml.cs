using MathChatBot.Models;
using MathChatBot.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for AdminControlsWindow.xaml
    /// </summary>
    public partial class AdminControlsWindow : Window, IWindowDataTransfer
    {

        /****************************************************************/
        // PROPERTIES
        /****************************************************************/
        #region Properties

        private User User { get; set; }
        private List<User> Users { get; set; }
        private MathChatBotEntities MathChatBotEntities { get; set; }

        #endregion

        /****************************************************************/
        // CONSTRUCTOR
        /****************************************************************/
        #region Constructor

        public AdminControlsWindow(User user)
        {
            InitializeComponent();

            // Get entity
            MathChatBotEntities = new MathChatBotEntities();
            // Get users
            Users = MathChatBotEntities.Users.ToList();
            // Order users
            Users = Users.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            // Set item source
            dgUsers.ItemsSource = Users;

            // Click events
            btnNewUser.Click += button_Click;
            btnAddUsersFromFile.Click += button_Click;

            // Setup top border header
            this.SetupBorderHeader(Properties.Resources.admin_controls_title);
        }

        #endregion

        /****************************************************************/
        // FUNCTIONS
        /****************************************************************/
        #region Functions

        /// <summary>
        /// A thread function to add users from file
        /// </summary>
        /// <param name="filePath">The file path to the users CSV file</param>
        private void AddUsersFromFile(object filePath)
        {
            // Flag for running thread
            bool runThread = true;

            try
            {
                this.RunOnUIThread(() =>
                {
                    CustomDialog.Show(Properties.Resources.adding_users_from_file_please_wait, CustomDialog.CustomDialogTypes.Progress, (o, e) =>
                    {
                        // Stop thread by removing flag
                        runThread = false;
                    });
                });

                // Parse CSV file and get a list with all the user entries
                var list = CSVUtility.ParseCSV((string)filePath);

                // For each entry in the CSV file
                foreach (var dictionary in list)
                {
                    // Stop if flag has been removed
                    if (!runThread)
                        break;

                    // Set values from file
                    var tempUser = new User();
                    // Set properties from file entry through reflection
                    CSVUtility.SetObjectValues(dictionary, tempUser);
                    // Create the user and get the user
                    var user = DatabaseUtility.CreateUser(tempUser);

                    // Check if we have a user
                    if (!(user is User))
                        continue;

                    // Add roles for the user
                    if (dictionary[nameof(MathChatBotEntities.Roles)] != null)
                    {
                        // Get roles from file entry
                        var roles = Regex.Split(dictionary[nameof(MathChatBotEntities.Roles)], ",");
                        // Add roles for the user
                        if (!DatabaseUtility.AddRolesByName(user, roles))
                            break;
                    }
                }

                // Stop if flag has been removed
                if (!runThread)
                    return;

                // Show message to the user
                this.RunOnUIThread(() =>
                {
                    CustomDialog.Show(Properties.Resources.successfully_gone_through_file);
                });
            }
            catch (Exception mes)
            {
                // Show error message to user
                this.RunOnUIThread(() =>
                {
                    CustomDialog.Show(mes.Message);
                });
            }
        }

        #endregion

        /****************************************************************/
        // EVENTS
        /****************************************************************/
        #region Events

        // Click
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                case nameof(btnNewUser):
                    {
                        //InputWindow inputWindow = new InputWindow(InputWindow.WindowTypes.NewUser);
                        //inputWindow.ShowDialog();
                        break;
                    }
                case nameof(btnAddUsersFromFile):
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            Thread thread = new Thread(new ParameterizedThreadStart(AddUsersFromFile));
                            thread.Start(openFileDialog.FileName);
                        }

                        break;
                    }
                case "btnMore":
                    {
                        User user = ((FrameworkElement)sender).DataContext as User;
                        var inputWindow = new InputWindow(MathChatBotEntities, this, user);
                        inputWindow.ShowDialog();
                        break;
                    }
            }
        }

        // RowEditEnding
        private void dataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                MathChatBotEntities.SaveChanges();
            }
            catch (Exception mes)
            {
                CustomDialog.Show(mes.Message);
            }
        }

        // CellEditEnding
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var flag = MathChatBotEntities.SaveChanges();
        }

        #endregion

        /****************************************************************/
        // INTERFACES
        /****************************************************************/
        #region Interfaces

        // IWindowDataTransfer - ReceivedData (Used with InputWindow)
        public void ReceivedData(InputWindow.WindowTypes windowType, object data)
        {
            switch (windowType)
            {
                case InputWindow.WindowTypes.NewUser:
                    break;
                case InputWindow.WindowTypes.ResetPassword:
                    break;
                case InputWindow.WindowTypes.UserInformation:
                    if (!(data is User))
                        return;

                    var user = (User)data;
                    var flag = MathChatBotEntities.SaveChanges();

                    break;
            }
        }

        #endregion

    }
}
