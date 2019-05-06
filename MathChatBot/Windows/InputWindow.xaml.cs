using MathChatBot.Models;
using MathChatBot.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MathChatBot
{

    public enum WindowTypes
    {
        NewUser,
        ResetPassword,
        UserInformation,
        MaterialOverview,
        ClassOverview,
        AddUsersToClass,
        SeeTopicDefinitions,
        SeeTermDefinitionsAndAssignments,
        SeeTermMaterialExamples,
        NewTopicDefinition,
        NewTermMaterial,
        NewTermMaterialExample,
        NewAssignment,
        NewTopic,
        NewTerm,
        OverwriteAssignment,
        OverwriteMaterial,
        OverwriteMaterialExample,
        SeeMaterial
    }

    public static class InputWindowHelper
    {
        public static void ShowInputWindow(this Window window, WindowTypes windowTypes, object obj, Action action = null)
        {
            InputWindow inputWindow = new InputWindow(windowTypes, obj);
            window.IsEnabled = false;
            inputWindow.Closing += (s, a) =>
            {
                window.IsEnabled = true;
                DatabaseUtility.RefreshEntity();
                action?.Invoke();
            };
            inputWindow.Show();
        }

        public static void ShowInputWindow(this Window window, WindowTypes windowTypes, object obj)
        {
            InputWindow inputWindow = new InputWindow(windowTypes, obj);
            window.IsEnabled = false;
            inputWindow.Closing += (s, a) =>
            {
                window.IsEnabled = true;
            };
            inputWindow.Show();
        }
    }

    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        private MathChatBotEntities Entity { get { return DatabaseUtility.Entity; } }
        private WindowTypes WindowType { get; set; }
        public User User { get; set; }
        public List<Role> RolesListForUser { get; set; }
        private Topic Topic { get; set; }
        private Term Term { get; set; }
        private Material Material { get; set; }
        private MaterialExample MaterialExample { get; set; }
        private Assignment Assignment { get; set; }
        private Class Class { get; set; }
        private string Source { get; set; }
        private string SelectedSource { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public InputWindow(WindowTypes windowTypes, object obj = null)
        {
            InitializeComponent();

            // Visibility
            spResetPassword.Visibility = Visibility.Collapsed;
            spUser.Visibility = Visibility.Collapsed;
            spClassOverview.Visibility = Visibility.Collapsed;
            spMaterial.Visibility = Visibility.Collapsed;
            spAddMaterial.Visibility = Visibility.Collapsed;
            spSeeMaterial.Visibility = Visibility.Collapsed;
            spNewTopic.Visibility = Visibility.Collapsed;
            spNewTerm.Visibility = Visibility.Collapsed;
            spAddAssignment.Visibility = Visibility.Collapsed;
            spOverwriteMaterial.Visibility = Visibility.Collapsed;
            dgAssignments.Visibility = Visibility.Collapsed;
            dgMaterial.Visibility = Visibility.Collapsed;

            // Click events
            btnOk.Click += button_Click;
            btnCancel.Click += button_Click;
            btnMiddle.Click += button_Click;
            btnSelectMaterial.Click += button_Click;

            WindowType = windowTypes;

            this.SetupBorderHeader();

            switch (WindowType)
            {
                // Reset password for user
                case WindowTypes.ResetPassword:
                    {
                        User = (User)obj;

                        // Visibility
                        spResetPassword.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.reset;

                        // Click events
                        btnUserResetPassword.Click += button_Click;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.reset_password);
                        break;
                    }
                // See user information    
                case WindowTypes.UserInformation:
                    {
                        User = (User)obj;

                        // Visibility
                        spUser.Visibility = Visibility.Visible;
                        lblUserPassword.Visibility = Visibility.Collapsed;
                        tbUserPassword.Visibility = Visibility.Collapsed;

                        // Texts
                        btnOk.Content = Properties.Resources.save;

                        // Get roles for user
                        RolesListForUser = DatabaseUtility.GetRolesListForUser(User.Username);
                        lbRoles.ItemsSource = RolesListForUser;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.user_information);
                        break;
                    }
                // New user
                case WindowTypes.NewUser:
                    {
                        User = new User();

                        // Visibility
                        spUser.Visibility = Visibility.Visible;
                        cbUserIsActivated.Visibility = Visibility.Collapsed;
                        btnUserResetPassword.Visibility = Visibility.Collapsed;
                        tbUserPassword.Visibility = Visibility.Visible;

                        // TextChanged events
                        tbUserFirstName.TextChanged += textBox_TextChanged;
                        tbUserLastName.TextChanged += textBox_TextChanged;

                        // PreviewTextInput events
                        tbUserFirstName.PreviewTextInput += textBox_PreviewTextInput;
                        tbUserLastName.PreviewTextInput += textBox_PreviewTextInput;

                        // Get roles for user
                        RolesListForUser = DatabaseUtility.GetRolesList();
                        lbRoles.ItemsSource = RolesListForUser;

                        // Texts
                        btnOk.Content = Properties.Resources.create;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.new_user);
                        break;
                    }
                // New topic
                case WindowTypes.NewTopic:
                    {
                        // Visibility
                        spNewTopic.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.new_topic);
                        break;
                    }
                // New term
                case WindowTypes.NewTerm:
                    {
                        // Visibility
                        spNewTerm.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        // Get all topics in the database
                        var topics = Entity.Topics.ToList();
                        cbbNewTermTopics.ItemsSource = topics;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.new_term);
                        break;
                    }
                // See class overview
                case WindowTypes.ClassOverview:
                    {
                        Class = (Class)obj;

                        // Visibility
                        spClassOverview.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        SeeUsersInClass();

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.class_overview, Class.Name));
                        break;
                    }
                case WindowTypes.AddUsersToClass:
                    {
                        Class = (Class)obj;

                        // Visibility
                        spClassOverview.Visibility = Visibility.Visible;
                        dgtcUsersRemove.Visibility = Visibility.Collapsed;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        dgUsers.SelectionMode = DataGridSelectionMode.Extended;

                        SeeUsersNotInClass();

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.add_users_to_class, Class.Name));
                        break;
                    }
                // See term definitions and assignments
                case WindowTypes.SeeTermDefinitionsAndAssignments:
                    {
                        Term = (Term)obj;

                        // Visibility
                        spMaterial.Visibility = Visibility.Visible;
                        dgtcSeeExamples.Visibility = Visibility.Visible;
                        gridMaterialAssignment.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        // Set combobox items
                        cbbMaterialAssignment.ItemsSource = new string[] { Properties.Resources.definitions, Properties.Resources.assignments };
                        cbbMaterialAssignment.SelectedItem = Properties.Resources.definitions;
                        cbbMaterialAssignment.SelectionChanged += (s, a) =>
                        {
                            var selection = cbbMaterialAssignment.SelectedItem.ToString();
                            if (selection == Properties.Resources.definitions)
                                SeeTermDefinitions();
                            else if (selection == Properties.Resources.assignments)
                                SeeAssignments();
                        };

                        // Show definitions for a term
                        SeeTermDefinitions();

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.defitions_and_assignments, Term.Name));
                        break;
                    }
                // See topic definitions
                case WindowTypes.SeeTopicDefinitions:
                    {
                        Topic = (Topic)obj;

                        // Visibility
                        spMaterial.Visibility = Visibility.Visible;
                        dgtcSeeExamples.Visibility = Visibility.Collapsed;
                        gridMaterialAssignment.Visibility = Visibility.Collapsed;
                        cbbMaterialAssignment.Visibility = Visibility.Collapsed;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        // Layout
                        dgMaterial.Width = 500;

                        // Show definitions for a topic
                        SeeTopicDefinitions();

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.topic_definitions, Topic.Name));
                        break;
                    }
                // Add new topic definition
                case WindowTypes.NewTopicDefinition:
                    {
                        Topic = (Topic)obj;

                        // Visibility
                        spAddMaterial.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.add;
                        lblMaterialName.Content = Topic.Name;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.new_topic_definition, Topic.Name));
                        break;
                    }
                // Add new assignment for a term
                case WindowTypes.NewAssignment:
                    {
                        Term = (Term)obj;

                        // Visibility
                        spAddAssignment.Visibility = Visibility.Visible;

                        // Texts
                        lblTermName.Content = Term.Name;
                        btnOk.Content = Properties.Resources.add;

                        // Click events
                        btnSelectAssignment.Click += button_Click;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.new_assignment, Term.Name));
                        break;
                    }
                // Add new term material
                case WindowTypes.NewTermMaterial:
                    {
                        Term = (Term)obj;

                        // Visibility
                        spAddMaterial.Visibility = Visibility.Visible;

                        // Texts
                        lblMaterialName.Content = Term.Name;
                        btnOk.Content = Properties.Resources.add;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.new_term_definition, Term.Name));
                        break;
                    }
                // See term material examples
                case WindowTypes.SeeTermMaterialExamples:
                    {
                        Material = (Material)obj;

                        // Visibility
                        spMaterial.Visibility = Visibility.Visible;
                        dgtcSeeExamples.Visibility = Visibility.Collapsed;
                        cbbMaterialAssignment.Visibility = Visibility.Collapsed;

                        // Texts
                        btnOk.Content = Properties.Resources.add;

                        // Show examples for a definition
                        SeeExamples();

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.term_defition_examples, Material.Term.Name));
                        break;
                    }
                // Add new term material example
                case WindowTypes.NewTermMaterialExample:
                    {
                        Material = (Material)obj;

                        // Visibility
                        spAddMaterial.Visibility = Visibility.Visible;

                        // Texts
                        lblMaterialName.Content = Material.Term.Name;
                        btnOk.Content = Properties.Resources.add;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.new_term_definition_example, Material.Term.Name));
                        break;
                    }
                // Overwrite assignments
                case WindowTypes.OverwriteAssignment:
                    {
                        Assignment = (Assignment)obj;

                        // Visibility
                        spAddMaterial.Visibility = Visibility.Visible;
                        spOverwriteMaterial.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.overwrite;
                        lblMaterialName.Content = Properties.Resources.new_image;
                        lblOverwriteMaterial.Content = Properties.Resources.old_image;

                        // Show current image
                        var currentImage = Utility.Base64ToImage(Assignment.Source);
                        imgOverwriteMaterial.Source = currentImage;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.overwrite_assignment, Assignment.Term.Name));
                        break;
                    }
                // Overwrite material    
                case WindowTypes.OverwriteMaterial:
                    {
                        Material = (Material)obj;

                        // Visibility
                        spAddMaterial.Visibility = Visibility.Visible;
                        spOverwriteMaterial.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.overwrite;
                        lblMaterialName.Content = Properties.Resources.new_image;
                        lblOverwriteMaterial.Content = Properties.Resources.old_image;

                        // Show current image
                        var currentImage = Utility.Base64ToImage(Material.Source);
                        imgOverwriteMaterial.Source = currentImage;

                        // Set border
                        if (Material.Term == null)
                            this.SetupBorderHeader(string.Format(Properties.Resources.overwrite_topic_definition, Material.Topic.Name));
                        else
                            this.SetupBorderHeader(string.Format(Properties.Resources.overwrite_term_definition, Material.Term.Name));
                        break;
                    }
                // Overwrite material example
                case WindowTypes.OverwriteMaterialExample:
                    {
                        MaterialExample = (MaterialExample)obj;

                        // Visilibity
                        spAddMaterial.Visibility = Visibility.Visible;
                        spOverwriteMaterial.Visibility = Visibility.Visible;

                        // Texts
                        btnOk.Content = Properties.Resources.overwrite;
                        lblMaterialName.Content = Properties.Resources.new_image;
                        lblOverwriteMaterial.Content = Properties.Resources.old_image;

                        // Show current image
                        var currentImage = Utility.Base64ToImage(MaterialExample.Source);
                        imgOverwriteMaterial.Source = currentImage;

                        // Set border
                        this.SetupBorderHeader(string.Format(Properties.Resources.overwrite_term_definition_example, MaterialExample.Material.Term.Name));
                        break;
                    }
                // See source material
                case WindowTypes.SeeMaterial:
                    {
                        Source = (string)obj;

                        // Visibility
                        spSeeMaterial.Visibility = Visibility.Visible;
                        btnOk.Visibility = Visibility.Collapsed;

                        // Texts
                        btnCancel.Content = Properties.Resources.close;

                        // Show image
                        var image = Utility.Base64ToImage(Source);
                        imgSeeMaterial.Source = image;

                        // Set border
                        this.SetupBorderHeader(Properties.Resources.see_image);
                        break;
                    }
            }
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// See definitions for a topic
        /// </summary>
        /// <param name="topicName">Name of the topic</param>
        private void SeeTopicDefinitions()
        {
            var materials = Entity.Materials
                .Where(x => x.Topic.Name == Topic.Name)
                .OrderBy(x => x.ShowOrderId)
                .ToList();
            dgMaterial.ItemsSource = materials;

            dgMaterial.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// See definitions for a term
        /// </summary>
        /// <param name="termName">Name of the term</param>
        private void SeeTermDefinitions()
        {
            var materials = Entity.Materials
                .Where(x => x.Term.Name == Term.Name)
                .OrderBy(x => x.ShowOrderId)
                .ToList();
            dgMaterial.ItemsSource = materials;

            dgAssignments.Visibility = Visibility.Collapsed;
            dgMaterial.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// See assignments for a term
        /// </summary>
        /// <param name="termName">Name of the term</param>
        private void SeeAssignments()
        {
            var assignments = Entity.Assignments
                .Where(x => x.Term.Name == Term.Name)
                .OrderBy(x => x.AssignmentNo)
                .ToList();
            dgAssignments.ItemsSource = assignments;

            dgAssignments.Visibility = Visibility.Visible;
            dgMaterial.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// See examples for a material
        /// </summary>
        /// <param name="material">The material object</param>
        private void SeeExamples()
        {
            var examples = Entity.MaterialExamples
                .Where(x => x.MaterialId == Material.Id)
                .ToList();
            dgMaterial.ItemsSource = examples;

            dgAssignments.Visibility = Visibility.Collapsed;
            dgMaterial.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Select image
        /// </summary>
        /// <param name="path">Return path for the image</param>
        /// <param name="source">Return source for the image</param>
        /// <param name="image">Return the actual image</param>
        /// <returns>True if success else false</returns>
        private bool SelectImage(out string path, out string source, out BitmapImage image)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;

                var dir = Path.GetDirectoryName(fileName);
                var tempFileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
                var tempFullPath = Path.Combine(dir, tempFileName);

                path = tempFullPath;
                source = Utility.ImageToBase64(fileName);
                image = Utility.Base64ToImage(source);
                return true;
            }

            path = null;
            source = null;
            image = null;

            return false;
        }

        private bool ChangeOrder<T>(bool up, List<T> list, T obj)
        {
            int index = list.IndexOf(obj);

            if (up)
            {
                if (index == 0)
                    return false;

                list.Remove(obj);
                list.Insert(index - 1, obj);
            }
            else
            {
                if (index == list.Count - 1)
                    return false;

                list.Remove(obj);
                list.Insert(index + 1, obj);
            }

            return true;
        }

        /// <summary>
        /// Get users in a class
        /// </summary>
        private void SeeUsersInClass()
        {
            var users = DatabaseUtility.GetUsersInClass(Class, new Role.RoleTypes[] { Role.RoleTypes.Student, Role.RoleTypes.Teacher }, true);
            // Show users
            dgUsers.ItemsSource = users;
        }

        private void SeeUsersNotInClass()
        {
            // Get users not in class
            var users = DatabaseUtility.GetUsersNotInClass(Class);
            // Show users
            dgUsers.ItemsSource = users;
        }

        #endregion

        //*************************************************/
        // EVENTS
        //*************************************************/
        #region Events

        // TextBox - PreviewTextInput
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]");
            e.Handled = regex.IsMatch(e.Text);
        }

        // TextBox - TextChanged
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            switch (tb.Name)
            {
                case nameof(tbUserFirstName):
                case nameof(tbUserLastName):
                    {
                        if (User.FirstName == null || User.FirstName.Length < 2 || User.LastName == null || User.LastName.Length < 2)
                            tbUserUsername.Text = string.Empty;
                        else
                        {
                            if (User.Username == null || User.Username == string.Empty)
                                tbUserUsername.Text = DatabaseUtility.GenerateUsername(User.FirstName, User.LastName);
                        }

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
                    case nameof(btnUserResetPassword):
                        {
                            InputWindow inputWindow = new InputWindow(WindowTypes.ResetPassword, User);
                            inputWindow.ShowDialog();
                            return;
                        }
                    case nameof(btnOk):
                        {
                            switch (WindowType)
                            {
                                // Create
                                case WindowTypes.NewUser:
                                    {
                                        if (User.FirstName == null || User.FirstName.Length < 2 || User.LastName == null || User.LastName.Length < 2 || User.Password == null || User.Password.Length < 8)
                                        {
                                            CustomDialog.Show(Properties.Resources.create_user_error);
                                            return;
                                        }

                                        User = DatabaseUtility.CreateUser(User);
                                        if (User is User)
                                        {
                                            foreach (var role in RolesListForUser)
                                            {
                                                if (role.IsAssigned)
                                                {
                                                    Entity.UserRoleRelations.Add(new UserRoleRelation()
                                                    {
                                                        UserId = User.Id,
                                                        RoleId = role.Id
                                                    });
                                                }
                                            }

                                            Entity.SaveChanges();
                                        }

                                        break;
                                    }
                                // Reset        
                                case WindowTypes.ResetPassword:
                                    {
                                        var newPassword = pbResetPasswordPassword.Password;
                                        var newPasswordAgain = pbResetPasswordPasswordAgain.Password;

                                        if (newPassword != newPasswordAgain)
                                        {
                                            CustomDialog.Show(Properties.Resources.the_passwords_need_to_match);
                                            return;
                                        }
                                        else if (newPassword.Length < 8)
                                        {
                                            CustomDialog.Show(Properties.Resources.the_password_has_to_be_at_least_eight_characters_long);
                                            return;
                                        }

                                        DatabaseUtility.ResetUserPassword(newPassword, User.Username);

                                        break;
                                    }
                                // Save
                                case WindowTypes.UserInformation:
                                    {
                                        var userRoleRelations = User.UserRoleRelations;

                                        foreach (var role in RolesListForUser)
                                        {
                                            var userRoleRelation = userRoleRelations.FirstOrDefault(x => x.RoleId == role.Id);

                                            if ((role.IsAssigned && userRoleRelation != null) || (!role.IsAssigned && userRoleRelation == null))
                                                continue;

                                            if (role.IsAssigned)
                                            {
                                                Entity.UserRoleRelations.Add(new UserRoleRelation()
                                                {
                                                    UserId = User.Id,
                                                    RoleId = role.Id
                                                });
                                            }
                                            else
                                                Entity.Entry(userRoleRelation).State = System.Data.Entity.EntityState.Deleted;
                                        }

                                        Entity.SaveChanges();

                                        break;
                                    }
                                // Add
                                case WindowTypes.ClassOverview:
                                    {
                                        this.ShowInputWindow(WindowTypes.AddUsersToClass, Class);
                                        return;
                                    }
                                // Add
                                case WindowTypes.NewTopicDefinition:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        var material = new Material()
                                        {
                                            TopicId = Topic.Id,
                                            Source = SelectedSource
                                        };
                                        Entity.Materials.Add(material);
                                        Entity.SaveChanges();
                                        break;
                                    }
                                // Add
                                case WindowTypes.NewTermMaterial:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        var material = new Material()
                                        {
                                            TermId = Term.Id,
                                            Source = SelectedSource
                                        };
                                        Entity.Materials.Add(material);
                                        Entity.SaveChanges();
                                        break;
                                    }
                                // Add
                                case WindowTypes.NewTermMaterialExample:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        var materialExample = new MaterialExample()
                                        {
                                            Source = SelectedSource,
                                            MaterialId = Material.Id
                                        };
                                        Entity.MaterialExamples.Add(materialExample);
                                        Entity.SaveChanges();
                                        break;
                                    }
                                // Add
                                case WindowTypes.NewAssignment:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        Func<string, string> func = (s) =>
                                        {
                                            return string.IsNullOrEmpty(s.Trim()) ? null : s;
                                        };

                                        var answerA = func(tbAssignmentAnswerA.Text);
                                        var answerB = func(tbAssignmentAnswerB.Text);
                                        var answerC = func(tbAssignmentAnswerC.Text);
                                        var answerD = func(tbAssignmentAnswerD.Text);
                                        var answerE = func(tbAssignmentAnswerE.Text);
                                        var answerF = func(tbAssignmentAnswerF.Text);
                                        var answerG = func(tbAssignmentAnswerG.Text);

                                        var assignment = new Assignment()
                                        {
                                            AnswerA = answerA,
                                            AnswerB = answerB,
                                            AnswerC = answerC,
                                            AnswerD = answerD,
                                            AnswerE = answerE,
                                            AnswerF = answerF,
                                            AnswerG = answerG,
                                            TermId = Term.Id
                                        };

                                        Entity.Assignments.Add(assignment);
                                        Entity.SaveChanges();

                                        SeeAssignments();

                                        break;
                                    }
                                // Add
                                case WindowTypes.SeeTermDefinitionsAndAssignments:
                                    {
                                        var selection = cbbMaterialAssignment.SelectedItem.ToString();
                                        if (selection == Properties.Resources.assignments)
                                        {
                                            this.ShowInputWindow(WindowTypes.NewAssignment, Term, () =>
                                            {
                                                SeeAssignments();
                                            });
                                        }
                                        else
                                        {
                                            this.ShowInputWindow(WindowTypes.NewTermMaterial, Term, () =>
                                            {
                                                SeeTermDefinitions();
                                            });
                                        }

                                        return;
                                    }
                                // Add
                                case WindowTypes.SeeTopicDefinitions:
                                    {
                                        this.ShowInputWindow(WindowTypes.NewTopicDefinition, Topic, () =>
                                        {
                                            SeeTopicDefinitions();
                                        });
                                        return;
                                    }
                                // Add
                                case WindowTypes.SeeTermMaterialExamples:
                                    {
                                        this.ShowInputWindow(WindowTypes.NewTermMaterialExample, Material, () =>
                                        {
                                            SeeExamples();
                                        });
                                        return;
                                    }
                                // Add
                                case WindowTypes.NewTopic:
                                    {
                                        var topicName = tbTopicName.Text;
                                        if (string.IsNullOrEmpty(topicName))
                                        {
                                            CustomDialog.Show(Properties.Resources.please_fill_out_all_fields);
                                            return;
                                        }

                                        if (!DatabaseUtility.AddNewTopic(topicName))
                                        {
                                            CustomDialog.Show(Properties.Resources.could_not_add_the_topic);
                                            return;
                                        }

                                        break;
                                    }
                                // Add
                                case WindowTypes.NewTerm:
                                    {
                                        var topic = (Topic)cbbNewTermTopics.SelectedItem;
                                        var termName = tbTermName.Text;
                                        if (topic == null || string.IsNullOrEmpty(termName))
                                        {
                                            CustomDialog.Show(Properties.Resources.please_fill_out_all_fields);
                                            return;
                                        }

                                        if (!DatabaseUtility.AddNewTerm(topic, termName))
                                        {
                                            CustomDialog.Show(Properties.Resources.could_not_add_the_term);
                                            return;
                                        }

                                        break;
                                    }
                                // Overwrite
                                case WindowTypes.OverwriteAssignment:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        Assignment.Source = SelectedSource;
                                        Entity.Entry(Assignment).State = System.Data.Entity.EntityState.Modified;
                                        Entity.SaveChanges();

                                        break;
                                    }
                                // Overwrite        
                                case WindowTypes.OverwriteMaterialExample:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        MaterialExample.Source = SelectedSource;
                                        Entity.Entry(MaterialExample).State = System.Data.Entity.EntityState.Modified;
                                        Entity.SaveChanges();

                                        break;
                                    }
                                // Overwrite
                                case WindowTypes.OverwriteMaterial:
                                    {
                                        if (SelectedSource == null)
                                        {
                                            CustomDialog.Show(Properties.Resources.please_select_an_image);
                                            return;
                                        }

                                        Material.Source = SelectedSource;
                                        Entity.Entry(Material).State = System.Data.Entity.EntityState.Modified;
                                        Entity.SaveChanges();

                                        break;
                                    }
                            }

                            Close();

                            break;
                        }
                    case nameof(btnCancel):
                        {
                            Close();
                            break;
                        }
                    case nameof(btnSelectMaterial):
                        {
                            string path;
                            string source;
                            BitmapImage image;
                            if (SelectImage(out path, out source, out image))
                            {
                                tbMaterialPath.Text = path;
                                SelectedSource = source;
                                imgAddMaterial.Source = image;
                            }

                            break;
                        }
                    case nameof(btnSelectAssignment):
                        {
                            string path;
                            string source;
                            BitmapImage image;
                            if (SelectImage(out path, out source, out image))
                            {
                                tbAssignmentPath.Text = path;
                                SelectedSource = source;
                                imgAddAssignment.Source = image;
                            }

                            break;
                        }
                    case "btnSee":
                        {
                            switch (WindowType)
                            {
                                case WindowTypes.SeeTopicDefinitions:
                                case WindowTypes.SeeTermDefinitionsAndAssignments:
                                case WindowTypes.SeeTermMaterialExamples:
                                    {
                                        var listObject = btn.DataContext;

                                        if (listObject is Material)
                                        {
                                            var material = (Material)listObject;
                                            this.ShowInputWindow(WindowTypes.SeeMaterial, material.Source);
                                        }
                                        else if (listObject is Assignment)
                                        {
                                            var assignment = (Assignment)listObject;
                                            this.ShowInputWindow(WindowTypes.SeeMaterial, assignment.Source);
                                        }
                                        else if (listObject is MaterialExample)
                                        {
                                            var materialExample = (MaterialExample)listObject;
                                            this.ShowInputWindow(WindowTypes.SeeMaterial, materialExample.Source);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    case "btnSeeExamples":
                        {
                            var material = (Material)btn.DataContext;
                            if (material == null)
                                return;

                            this.ShowInputWindow(WindowTypes.SeeTermMaterialExamples, material, () =>
                            {
                                SeeTermDefinitions();
                            });

                            break;
                        }
                    case "btnOverwrite":
                        {
                            var listObject = btn.DataContext;

                            if (listObject is Material)
                            {
                                this.ShowInputWindow(WindowTypes.OverwriteMaterial, listObject, () =>
                                {
                                    switch (WindowType)
                                    {
                                        case WindowTypes.SeeTermDefinitionsAndAssignments:
                                            {
                                                SeeTermDefinitions();
                                                break;
                                            }
                                        case WindowTypes.SeeTopicDefinitions:
                                            {
                                                SeeTopicDefinitions();
                                                break;
                                            }
                                    }
                                });
                            }
                            else if (listObject is MaterialExample)
                            {
                                this.ShowInputWindow(WindowTypes.OverwriteMaterialExample, listObject, () =>
                                {
                                    SeeExamples();
                                });
                            }
                            else if (listObject is Assignment)
                            {
                                this.ShowInputWindow(WindowTypes.OverwriteAssignment, listObject, () =>
                                {
                                    SeeAssignments();
                                });
                            }

                            break;
                        }
                    case "btnRemove":
                        {
                            var listObject = btn.DataContext;

                            if (listObject is Material)
                            {
                                var material = (Material)listObject;
                                if (material.MaterialExamples.Any())
                                {
                                    CustomDialog.Show(Properties.Resources.could_not_remove_definition);
                                    return;
                                }
                            }

                            string message = "";

                            switch (WindowType)
                            {
                                case WindowTypes.SeeTopicDefinitions:
                                    {
                                        message = Properties.Resources.remove_definition;
                                        break;
                                    }
                                case WindowTypes.SeeTermDefinitionsAndAssignments:
                                    {
                                        if (listObject is Material)
                                            message = Properties.Resources.remove_definition;
                                        else if (listObject is Assignment)
                                            message = Properties.Resources.remove_assignment;

                                        break;
                                    }
                                case WindowTypes.ClassOverview:
                                    {
                                        message = Properties.Resources.remove_user_from_class;
                                        break;
                                    }
                            }

                            if (CustomDialog.ShowQuestion(message, CustomDialogQuestionTypes.YesNo) == CustomDialogQuestionResult.Yes)
                            {
                                // Remove definition from term or topic
                                if (listObject is Material)
                                {
                                    var material = (Material)listObject;
                                    var materials = dgMaterial.ItemsSource as List<Material>;
                                    materials.Remove(material);
                                    Entity.Entry(material).State = System.Data.Entity.EntityState.Deleted;

                                    foreach (var m in materials)
                                    {
                                        m.ShowOrderId = materials.IndexOf(m) + 1;
                                        Entity.Entry(m).State = System.Data.Entity.EntityState.Modified;
                                    }

                                    Entity.SaveChanges();

                                    switch (WindowType)
                                    {
                                        case WindowTypes.SeeTermDefinitionsAndAssignments:
                                            {
                                                SeeTermDefinitions();
                                                break;
                                            }
                                        case WindowTypes.SeeTopicDefinitions:
                                            {
                                                SeeTopicDefinitions();
                                                break;
                                            }
                                    }
                                }
                                // Remove example for definition
                                else if (listObject is MaterialExample)
                                {
                                    var example = (MaterialExample)listObject;
                                    var examples = (List<MaterialExample>)dgMaterial.ItemsSource;
                                    examples.Remove(example);
                                    Entity.Entry(example).State = System.Data.Entity.EntityState.Deleted;

                                    foreach (var ex in examples)
                                    {
                                        ex.ShowOrderId = examples.IndexOf(ex) + 1;
                                        Entity.Entry(ex).State = System.Data.Entity.EntityState.Modified;
                                    }

                                    Entity.SaveChanges();

                                    SeeExamples();
                                }
                                // Remove assignment
                                else if (listObject is Assignment)
                                {
                                    var assignment = (Assignment)listObject;
                                    var assignments = (List<Assignment>)dgAssignments.ItemsSource;
                                    assignments.Remove(assignment);
                                    Entity.Entry(assignment).State = System.Data.Entity.EntityState.Deleted;

                                    foreach (var a in assignments)
                                    {
                                        a.AssignmentNo = assignments.IndexOf(a) + 1;
                                        Entity.Entry(a).State = System.Data.Entity.EntityState.Modified;
                                    }

                                    Entity.SaveChanges();

                                    SeeAssignments();
                                }
                                // Remove user from class
                                else if (listObject is User)
                                {
                                    var user = (User)listObject;

                                    var userClassRelation = Class.UserClassRelations.FirstOrDefault(x => x.UserId == user.Id);
                                    Class.UserClassRelations.Remove(userClassRelation);
                                    Entity.Entry(userClassRelation).State = System.Data.Entity.EntityState.Deleted;

                                    var flag = Entity.SaveChanges();
                                    
                                    SeeUsersInClass();
                                }
                            }

                            break;
                        }
                    case "btnUp":
                    case "btnDown":
                        {
                            bool up = btn.Name == "btnUp";
                            var listObject = btn.DataContext;

                            if (listObject is Material)
                            {
                                var materials = (List<Material>)dgMaterial.ItemsSource;
                                var material = (Material)listObject;
                                if (ChangeOrder(up, materials, material))
                                {
                                    foreach (var m in materials)
                                    {
                                        m.ShowOrderId = materials.IndexOf(m) + 1;
                                        Entity.Entry(m).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                            else if (listObject is MaterialExample)
                            {
                                var examples = (List<MaterialExample>)dgMaterial.ItemsSource;
                                var example = (MaterialExample)listObject;
                                if (ChangeOrder(up, examples, example))
                                {
                                    foreach (var ex in examples)
                                    {
                                        ex.ShowOrderId = examples.IndexOf(ex) + 1;
                                        Entity.Entry(ex).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                            else if (listObject is Assignment)
                            {
                                var assignments = (List<Assignment>)dgAssignments.ItemsSource;
                                var assignment = (Assignment)listObject;
                                if (ChangeOrder(up, assignments, assignment))
                                {
                                    foreach (var a in assignments)
                                    {
                                        a.AssignmentNo = assignments.IndexOf(a) + 1;
                                        Entity.Entry(a).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }

                            Entity.SaveChanges();

                            switch (WindowType)
                            {
                                case WindowTypes.SeeTopicDefinitions:
                                    {
                                        SeeTopicDefinitions();
                                        break;
                                    }
                                case WindowTypes.SeeTermDefinitionsAndAssignments:
                                    {
                                        var selection = cbbMaterialAssignment.SelectedItem.ToString();
                                        if (selection == Properties.Resources.assignments)
                                            SeeAssignments();
                                        else
                                            SeeTermDefinitions();

                                        break;
                                    }
                                case WindowTypes.SeeTermMaterialExamples:
                                    {
                                        SeeExamples();
                                        break;
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

        // DataGrid - RowEditEnding
        private void dataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                Entity.SaveChanges();
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
                Entity.SaveChanges();
            }
            catch (Exception mes)
            {
                CustomDialog.Show(mes.Message);
            }
        }

        #endregion

    }
}
