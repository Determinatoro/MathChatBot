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
    public partial class AdminControlsWindow : Window
    {

        /****************************************************************/
        // PROPERTIES
        /****************************************************************/
        #region Properties

        private User User { get; set; }
        private List<User> Users { get; set; }
        public List<Class> Classes { get; set; }
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
            GetUsers();
            // Get classes
            GetClasses();

            // Click events
            btnNewUser.Click += button_Click;
            btnAddUsersFromFile.Click += button_Click;

            // TextChanged events
            tbSearchForUsers.TextChanged += textBox_TextChanged;

            // Setup top border header
            this.SetupBorderHeader(Properties.Resources.admin_controls_title);
        }

        #endregion

        /****************************************************************/
        // FUNCTIONS
        /****************************************************************/
        #region Functions

        /// <summary>
        /// Get users to show in the datagrid
        /// </summary>
        private void GetUsers()
        {
            // Get users
            Users = MathChatBotEntities.Users.ToList();
            // Order users
            Users = Users.OrderBy(x => x.FirstName,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ThenBy(x => x.LastName,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            // Set item source
            dgUsers.ItemsSource = Users;

            textBox_TextChanged(tbSearchForUsers, null);
        }

        /// <summary>
        /// Get classes to show in the datagrid
        /// </summary>
        private void GetClasses()
        {
            // Get classes
            Classes = MathChatBotEntities.Classes.ToList();
            // Order classes
            Classes = Classes.OrderBy(x => x.Name,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            dgClasses.ItemsSource = Classes;
        }

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
                // Parse CSV file and get a list with all the user entries
                var list = CSVUtility.ParseCSV((string)filePath);

                this.RunOnUIThread(() =>
                {
                    // Show progress dialog
                    CustomDialog.ShowProgress(Properties.Resources.adding_users_from_file_please_wait, list.Count, (o, e) =>
                    {
                        // Stop thread by removing flag
                        runThread = false;
                    });
                });

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

                    // Add user to classes
                    if (dictionary[nameof(MathChatBotEntities.Classes)] != null)
                    {
                        // Get roles from file entry
                        var classes = Regex.Split(dictionary[nameof(MathChatBotEntities.Classes)], ",");
                        // Add roles for the user
                        if (!DatabaseUtility.AddToClassesByName(user, classes))
                            break;
                    }

                    // Increment progress by one
                    this.RunOnUIThread(() =>
                    {
                        CustomDialog.IncrementProgress(1);
                    });
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

        // TextBox - Textchanged
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            switch (tb.Name)
            {
                case nameof(tbSearchForUsers):
                    {
                        var searchStr = tb.Text.ToLower();

                        // Shows all users if the search string is empty
                        if (searchStr == string.Empty)
                            dgUsers.ItemsSource = Users;
                        // Shows only the users which first name, last name or username 
                        // contains the search string
                        else
                            dgUsers.ItemsSource = Users.Where(x => 
                            x.FirstName.ToLower().Contains(searchStr) ||
                            x.LastName.ToLower().Contains(searchStr) ||
                            x.Username.ToLower().Contains(searchStr)
                            ).ToList();

                        break;
                    }
            }
        }

        // Button - Click
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                // New user
                case nameof(btnNewUser):
                    {
                        InputWindow inputWindow = new InputWindow(windowType: InputWindow.WindowTypes.NewUser);
                        // Closing event
                        inputWindow.Closing += window_Closing;
                        // Show the new user window
                        inputWindow.ShowDialog();
                        break;
                    }
                // Add users from file
                case nameof(btnAddUsersFromFile):
                    {
                        // Open a file dialog to select a CSV file
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            // Going through the file in a thread to avoid blocking the UI thread
                            Thread thread = new Thread(new ParameterizedThreadStart(AddUsersFromFile));
                            thread.Start(openFileDialog.FileName);
                        }

                        break;
                    }
                // More button for each user object in the datagrid
                case "btnMore":
                    {
                        // Get the associated object
                        User user = ((FrameworkElement)sender).DataContext as User;
                        // Open a window containing more information about the user
                        var inputWindow = new InputWindow(user, InputWindow.WindowTypes.UserInformation);
                        // Closing event
                        inputWindow.Closing += window_Closing;
                        // Show the information window
                        inputWindow.ShowDialog();
                        break;
                    }
                case "btnSeeUsers":
                    {
                        // Get the associated object
                        Class clas = ((FrameworkElement)sender).DataContext as Class;
                        var inputWindow = new InputWindow(clas);
                        // Closing event
                        inputWindow.Closing += window_Closing;
                        // Show the information window
                        inputWindow.ShowDialog();
                        break;
                    }

            }
        }

        // Window - Closing
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GetUsers();   
        }

        // DataGrid - RowEditEnding
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

        // DataGrid - CellEditEnding
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var flag = MathChatBotEntities.SaveChanges();
        }

        #endregion

    }
}
