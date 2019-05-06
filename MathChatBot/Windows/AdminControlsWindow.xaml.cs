using MathChatBot.Models;
using MathChatBot.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
        public List<Topic> Topics { get; set; }
        public List<Term> Terms { get; set; }

        private MathChatBotEntities Entity { get { return DatabaseUtility.Entity; } }

        #endregion

        /****************************************************************/
        // CONSTRUCTOR
        /****************************************************************/
        #region Constructor

        public AdminControlsWindow()
        {
            InitializeComponent();

            // Get topics
            var topics = Entity.Topics.Select(x => x.Name).ToList();
            // Insert a selection for all topics
            topics.Insert(0, Properties.Resources.all_topics);
            cbbTopics.ItemsSource = topics;
            cbbTopics.SelectedIndex = 0;

            // Click events
            btnNewUser.Click += button_Click;
            btnAddUsersFromFile.Click += button_Click;
            btnNewTopic.Click += button_Click;
            btnNewTerm.Click += button_Click;

            // TextChanged events
            tbSearchForUsers.TextChanged += textBox_TextChanged;
            tbSearchForClasses.TextChanged += textBox_TextChanged;

            // SelectionChanged events
            cbbTopics.SelectionChanged += control_SelectionChanged;

            // Setup top border header
            this.SetupBorderHeader(Properties.Resources.admin_controls_title);

            Loaded += window_Loaded;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get users
            GetUsers();
            // Get classes
            GetClasses();
            // Get topics
            GetTopics();
        }

        private void control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(cbbTopics):
                    {
                        var selectedItem = cbbTopics.SelectedItem.ToString();
                        CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait);

                        if (selectedItem == Properties.Resources.all_topics)
                            GetTopics();
                        else
                            GetTerms(selectedItem);
                        break;
                    }
            }
        }

        #endregion

        /****************************************************************/
        // METHODS
        /****************************************************************/
        #region Methods

        /// <summary>
        /// Get users to show in the datagrid
        /// </summary>
        private void GetUsers()
        {
            // Get users
            Users = Entity.Users.ToList();
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
            Classes = Entity.Classes.ToList();
            // Order classes
            Classes = Classes.OrderBy(x => x.Name,
                Comparer<string>.Create((x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            dgClasses.ItemsSource = Classes;
        }

        /// <summary>
        /// Get topics to show in the datagrid
        /// </summary>
        private void GetTopics()
        {
            this.StartThread(() =>
            {
                Topics = DatabaseUtility.Entity.Topics.OrderBy(x => x.Name).ToList();

                this.RunOnUIThread(() =>
                {
                    if (Topics.Count == 0)
                        CustomDialog.Dismiss();
                    dgTopics.ItemsSource = Topics;
                    dgTopics.Visibility = Visibility.Visible;
                    dgTerms.Visibility = Visibility.Collapsed;
                });
            });
        }

        /// <summary>
        /// Get topics to show in the datagrid
        /// </summary>
        private void GetTerms(string topicName)
        {
            this.StartThread(() =>
            {
                Terms = DatabaseUtility.GetTermInformations(topicName);

                this.RunOnUIThread(() =>
                {
                    if (!Terms.Any())
                        CustomDialog.Dismiss();
                    dgTerms.ItemsSource = Terms;
                    dgTopics.Visibility = Visibility.Collapsed;
                    dgTerms.Visibility = Visibility.Visible;
                });
            });
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
                    // Generate username
                    var username = DatabaseUtility.GenerateUsername(tempUser.FirstName, tempUser.LastName);
                    // Set properties from file entry through reflection
                    CSVUtility.SetObjectValues(dictionary, tempUser);
                    // Set username
                    tempUser.Username = username;
                    // Create the user and get the user
                    var user = DatabaseUtility.CreateUser(tempUser);

                    // Check if we have a user
                    if (!(user is User))
                        continue;

                    // Add roles for the user
                    if (dictionary[nameof(Entity.Roles)] != null)
                    {
                        // Get roles from file entry
                        var roles = Regex.Split(dictionary[nameof(Entity.Roles)], ",");
                        // Add roles for the user
                        if (!DatabaseUtility.AssignRolesByName(user, roles))
                            break;
                    }

                    // Add user to classes
                    if (dictionary[nameof(Entity.Classes)] != null)
                    {
                        // Get roles from file entry
                        var classes = Regex.Split(dictionary[nameof(Entity.Classes)], ",");
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
                case nameof(tbSearchForClasses):
                    {
                        var searchStr = tb.Text.ToLower();

                        // Show all classes if the search string is empty
                        if (searchStr == string.Empty)
                            dgClasses.ItemsSource = Classes;
                        // Show only classes which name contains the search string
                        else
                            dgClasses.ItemsSource = Classes.Where(x => x.Name.Contains(searchStr)).ToList();

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
                        InputWindow inputWindow = new InputWindow(WindowTypes.NewUser);
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
                // New topic
                case nameof(btnNewTopic):
                    {
                        InputWindow inputWindow = new InputWindow(WindowTypes.NewTopic);
                        inputWindow.Show();
                        IsEnabled = false;
                        inputWindow.Closing += window_Closing;

                        break;
                    }
                // New term
                case nameof(btnNewTerm):
                    {
                        InputWindow inputWindow = new InputWindow(WindowTypes.NewTerm);
                        inputWindow.Show();
                        IsEnabled = false;
                        inputWindow.Closing += window_Closing;

                        break;
                    }
                // More button for each user object in the datagrid
                case "btnMore":
                    {
                        // Get the associated object
                        User user = ((FrameworkElement)sender).DataContext as User;
                        // Open a window containing more information about the user
                        var inputWindow = new InputWindow(WindowTypes.UserInformation, user);
                        // Closing event
                        inputWindow.Closing += window_Closing;
                        // Show the information window
                        inputWindow.ShowDialog();
                        break;
                    }
                // Class object "See users"
                case "btnSeeUsers":
                    {
                        // Get the associated object
                        Class @class = ((FrameworkElement)sender).DataContext as Class;
                        var inputWindow = new InputWindow(WindowTypes.ClassOverview, @class);
                        // Closing event
                        inputWindow.Closing += window_Closing;
                        // Show the information window
                        inputWindow.ShowDialog();
                        break;
                    }
                // Term and Topic object "Edit"
                case "btnEdit":
                    {
                        var listObject = btn.DataContext;

                        var selectedItem = cbbTopics.SelectedItem.ToString();

                        if (listObject is Topic)
                        {
                            Topic topic = listObject as Topic;
                            this.ShowInputWindow(WindowTypes.SeeTopicDefinitions, topic, () =>
                            {
                                GetTopics();
                            });
                        }
                        else if (listObject is Term)
                        {
                            Term term = listObject as Term;
                            this.ShowInputWindow(WindowTypes.SeeTermDefinitionsAndAssignments, term, () =>
                            {
                                var topicName = cbbTopics.SelectedItem.ToString();
                                GetTerms(topicName);
                            });
                        }

                        break;
                    }
                // Term and Topic object "Remove"
                case "btnRemove":
                    {
                        // Get the object the user want to remove
                        var listObject = btn.DataContext;

                        // Get combobox selection
                        var selectedItem = cbbTopics.SelectedItem.ToString();

                        if (listObject is Topic)
                        {
                            var topic = listObject as Topic;

                            // Ask user if they want to remove topic
                            if (CustomDialog.ShowQuestion(string.Format(Properties.Resources.do_you_want_to_delete, topic.Name), CustomDialogQuestionTypes.YesNo) == CustomDialogQuestionResult.Yes)
                            {
                                if (!DatabaseUtility.DeleteTopic(topic))
                                    CustomDialog.Show(Properties.Resources.could_not_remove_the_topic);
                                else
                                {
                                    CustomDialog.Show(string.Format(Properties.Resources.item_removed, topic.Name));
                                    GetTopics();
                                }
                            }
                        }
                        else if (listObject is Term)
                        {
                            var term = listObject as Term;

                            // Ask user if they want to remove term
                            if (CustomDialog.ShowQuestion(string.Format(Properties.Resources.do_you_want_to_delete, term.Name), CustomDialogQuestionTypes.YesNo) == CustomDialogQuestionResult.Yes)
                            {
                                if (!DatabaseUtility.DeleteTerm(term))
                                    CustomDialog.Show(Properties.Resources.could_not_remove_the_term);
                                else
                                {
                                    CustomDialog.Show(string.Format(Properties.Resources.item_removed, term.Name));
                                    GetTerms(selectedItem);
                                }
                            }
                        }

                        break;
                    }

            }
        }

        // Window - Closing
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsEnabled = true;
            DatabaseUtility.RefreshEntity();

            GetUsers();
            GetClasses();

            var selectedTopic = cbbTopics.SelectedItem.ToString();
            if (selectedTopic == Properties.Resources.all_topics)
                GetTopics();
            else
                GetTerms(selectedTopic);
        }

        // DataGrid - RowEditEnding
        private void dataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                Entity.SaveChanges();
            }
            catch { }
        }

        // DataGrid - CellEditEnding
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                Entity.SaveChanges();
            }
            catch { }
        }

        // Any - Loaded
        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            CustomDialog.Dismiss();
        }

        #endregion

    }
}
