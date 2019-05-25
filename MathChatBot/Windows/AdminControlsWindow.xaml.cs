using MathChatBot.Models;
using MathChatBot.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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

            SetComboBoxTopics();

            // Click events
            btnNewUser.Click += button_Click;
            btnAddUsersFromFile.Click += button_Click;
            btnNewTopic.Click += button_Click;
            btnNewTerm.Click += button_Click;
            btnNewClass.Click += button_Click;

            // TextChanged events
            tbSearchForUsers.TextChanged += textBox_TextChanged;
            tbSearchForClasses.TextChanged += textBox_TextChanged;

            // SelectionChanged events
            cbbTopics.SelectionChanged += control_SelectionChanged;

            // Setup top border header
            this.SetupBorderHeader(Properties.Resources.admin_controls_title);

            Loaded += window_Loaded;
        }

        private void SetComboBoxTopics()
        {
            // Get topics
            var topics = Entity.Topics.Select(x => x.Name).ToList();
            // Insert a selection for all topics
            topics.Insert(0, Properties.Resources.all_topics);
            cbbTopics.ItemsSource = topics;
            cbbTopics.SelectedIndex = 0;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            GetData();
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

        private void GetData()
        {
            GetUsers();
            GetClasses();

            var selectedTopic = cbbTopics.SelectedItem.ToString();
            if (selectedTopic == Properties.Resources.all_topics)
                GetTopics();
            else
                GetTerms(selectedTopic);
        }

        /// <summary>
        /// Get users to show in the datagrid
        /// </summary>
        private void GetUsers()
        {
            Users = Entity.GetUsersOrderedAlphabetically();
            dgUsers.ItemsSource = Users;
            // Do filtering
            textBox_TextChanged(tbSearchForUsers, null);
        }

        /// <summary>
        /// Get classes to show in the datagrid
        /// </summary>
        private void GetClasses()
        {
            Classes = Entity.GetClassesOrderedAlphabetically();
            dgClasses.ItemsSource = Classes;
            // Do filtering
            textBox_TextChanged(tbSearchForClasses, null);
        }

        /// <summary>
        /// Get topics to show in the datagrid
        /// </summary>
        private void GetTopics()
        {
            this.StartThread(() =>
            {
                Topics = DatabaseUtility.Entity.Topics
                .OrderBy(x => x.Name)
                .ToList();

                this.RunOnUIThread(() =>
                {
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
                Terms = Entity.GetTermInformations(topicName);

                this.RunOnUIThread(() =>
                {
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

                // Show progress dialog
                CustomDialog.ShowProgress(Properties.Resources.adding_users_from_file_please_wait, list.Count, (o, e) =>
                {
                    // Stop thread by removing flag
                    runThread = false;
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
                    // Generate username
                    if (tempUser.Username == null)
                        tempUser.Username = Entity.GenerateUsername(tempUser.FirstName, tempUser.LastName);
                    // Create the user and get the user
                    var user = Entity.CreateUser(tempUser);

                    // Check if we have a user
                    if (!(user is User))
                    {
                        // Increment progress by one
                        CustomDialog.IncrementProgress(1);
                        continue;
                    }

                    // Add roles for the user
                    if (dictionary[nameof(Entity.Roles)] != null)
                    {
                        // Get roles from file entry
                        var roles = Regex.Split(dictionary[nameof(Entity.Roles)], ",");
                        // Add roles for the user
                        if (!Entity.AssignRolesByName(user, new HashSet<string>(roles)))
                            break;
                    }

                    // Add user to classes
                    if (dictionary[nameof(Entity.Classes)] != null)
                    {
                        // Get roles from file entry
                        var classes = Regex.Split(dictionary[nameof(Entity.Classes)], ",");
                        // Add roles for the user
                        if (!Entity.AddToClassesByName(user, new HashSet<string>(classes)))
                            break;
                    }

                    // Increment progress by one
                    CustomDialog.IncrementProgress(1);
                }

                // Stop if flag has been removed
                if (!runThread)
                    return;

                // Show message to the user
                CustomDialog.Show(Properties.Resources.successfully_gone_through_file);

                this.RunOnUIThread(() =>
                {
                    GetData();
                });
            }
            catch (Exception mes)
            {
                // Show error message to user
                CustomDialog.Show(mes.Message);
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
            try
            {
                var btn = sender as Button;

                switch (btn.Name)
                {
                    // New user
                    case nameof(btnNewUser):
                        {
                            this.ShowInputWindow(WindowTypes.NewUser, action: () =>
                            {
                                GetData();
                            });
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
                                this.StartThread(() =>
                                {
                                    AddUsersFromFile(openFileDialog.FileName);
                                });
                            }

                            break;
                        }
                    // New topic
                    case nameof(btnNewTopic):
                        {
                            this.ShowInputWindow(WindowTypes.NewTopic, action: () =>
                            {
                                GetData();
                            });

                            break;
                        }
                    // New term
                    case nameof(btnNewTerm):
                        {
                            this.ShowInputWindow(WindowTypes.NewTerm, action: () =>
                            {
                                GetData();
                            });

                            break;
                        }
                    // New class
                    case nameof(btnNewClass):
                        {
                            this.ShowInputWindow(WindowTypes.NewClass, action: () =>
                            {
                                GetData();
                            });

                            break;
                        }
                    // More button for each user object in the datagrid
                    case "btnMore":
                        {
                            // Get the associated object
                            User user = ((FrameworkElement)sender).DataContext as User;

                            this.ShowInputWindow(WindowTypes.UserInformation, user, () =>
                            {
                                GetData();
                            });
                            break;
                        }
                    // Class object "See users"
                    case "btnSeeUsers":
                        {
                            // Get the associated object
                            Class @class = ((FrameworkElement)sender).DataContext as Class;
                            this.ShowInputWindow(WindowTypes.ClassOverview, @class, () =>
                            {
                                GetData();
                            });
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
                                    GetData();
                                });
                            }
                            else if (listObject is Term)
                            {
                                Term term = listObject as Term;
                                this.ShowInputWindow(WindowTypes.SeeTermDefinitionsAndAssignments, term, () =>
                                {
                                    GetData();
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
                                    if (!Entity.DeleteTopic(topic))
                                        CustomDialog.Show(Properties.Resources.could_not_remove_the_topic);
                                    else
                                        GetTopics();
                                }
                            }
                            else if (listObject is Term)
                            {
                                var term = listObject as Term;

                                // Ask user if they want to remove term
                                if (CustomDialog.ShowQuestion(string.Format(Properties.Resources.do_you_want_to_delete, term.Name), CustomDialogQuestionTypes.YesNo) == CustomDialogQuestionResult.Yes)
                                {
                                    if (!Entity.DeleteTerm(term))
                                        CustomDialog.Show(Properties.Resources.could_not_remove_the_term);
                                    else
                                        GetTerms(selectedItem);
                                }
                            }

                            break;
                        }
                }
            }
            catch (Exception mes)
            {
                CustomDialog.Show(mes.Message);
            }
        }

        // DataGrid - CellEditEnding
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var dataGrid = sender as DataGrid;

                if (dataGrid.SelectedValue != null)
                    Entity.Entry(dataGrid.SelectedValue).State = System.Data.Entity.EntityState.Modified;

                if (Entity.ChangeTracker.HasChanges())
                {
                    Entity.SaveChanges();

                    var dg = sender as DataGrid;
                    if (dg == dgTopics)
                    {
                        SetComboBoxTopics();
                        GetTopics();
                    }
                }
            }
            catch (Exception mes)
            {
                CustomDialog.Show(mes.Message);
            }
        }

        #endregion

    }
}
