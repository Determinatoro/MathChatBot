using MathChatBot.Models;
using MathChatBot.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MathChatBot
{

    public enum WindowTypes
    {
        NewUser,
        ResetPassword,
        UserInformation,
        ClassOverview
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

        private WindowTypes WindowType { get; set; }
        public User User { get; set; }
        public List<Role> RolesListForUser { get; set; }
        private MathChatBotEntities Entity { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        /// <summary>
        /// Basic constructor
        /// </summary>
        private InputWindow()
        {
            InitializeComponent();

            Entity = DatabaseUtility.GetEntity();

            spResetPassword.Visibility = Visibility.Collapsed;
            spUser.Visibility = Visibility.Collapsed;
            spClassOverview.Visibility = Visibility.Collapsed;

            // Click events
            btnOk.Click += button_Click;
            btnCancel.Click += button_Click;
        }

        /// <summary>
        /// Constructor for reset password and user information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="windowType"></param>
        public InputWindow(User user, WindowTypes windowType) : this()
        {
            WindowType = windowType;
            User = Entity.Users.FirstOrDefault(x => x.Id == user.Id);

            switch (WindowType)
            {
                case WindowTypes.ResetPassword:
                    {
                        spResetPassword.Visibility = Visibility.Visible;

                        btnOk.Content = Properties.Resources.reset;

                        this.SetupBorderHeader(Properties.Resources.reset_password);

                        break;
                    }
                case WindowTypes.UserInformation:
                    {
                        spUser.Visibility = Visibility.Visible;

                        lblUserPassword.Visibility = Visibility.Collapsed;
                        tbUserPassword.Visibility = Visibility.Collapsed;

                        RolesListForUser = DatabaseUtility.GetRolesListForUser(user.Username);
                        lbRoles.ItemsSource = RolesListForUser;

                        btnUserResetPassword.Click += button_Click;

                        btnOk.Content = Properties.Resources.save;

                        this.SetupBorderHeader(Properties.Resources.user_information);
                        break;
                    }
            }
        }

        /// <summary>
        /// Constructor for new user 
        /// </summary>
        /// <param name="windowType"></param>
        public InputWindow(WindowTypes windowType) : this()
        {
            WindowType = windowType;

            switch (WindowType)
            {
                case WindowTypes.NewUser:
                    {
                        spUser.Visibility = Visibility.Visible;

                        User = new User();

                        cbUserIsActivated.Visibility = Visibility.Collapsed;
                        btnUserResetPassword.Visibility = Visibility.Collapsed;
                        tbUserPassword.Visibility = Visibility.Visible;

                        // TextChanged events
                        tbUserFirstName.TextChanged += textBox_TextChanged;
                        tbUserLastName.TextChanged += textBox_TextChanged;

                        // PreviewTextInput events
                        tbUserFirstName.PreviewTextInput += textBox_PreviewTextInput;
                        tbUserLastName.PreviewTextInput += textBox_PreviewTextInput;

                        RolesListForUser = DatabaseUtility.GetRolesList();
                        lbRoles.ItemsSource = RolesListForUser;

                        btnOk.Content = Properties.Resources.create;

                        this.SetupBorderHeader(Properties.Resources.new_user);
                        break;
                    }
            }

        }

        /// <summary>
        /// Constuctor for class overview
        /// </summary>
        /// <param name="clas">The class object</param>
        public InputWindow(Class clas) : this()
        {
            spClassOverview.Visibility = Visibility.Visible;

            WindowType = WindowTypes.ClassOverview;

            var users = Entity.UserClassRelations.Where(x => x.ClassId == clas.Id).Select(x => x.User).ToList();
            users.SetStringRolesForUsers();

            dgUsers.ItemsSource = users;

            btnOk.Visibility = Visibility.Collapsed;

            this.SetupBorderHeader(clas.Name);
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
            var btn = sender as Button;
            switch (btn.Name)
            {
                case nameof(btnUserResetPassword):
                    {
                        InputWindow inputWindow = new InputWindow(User, WindowTypes.ResetPassword);
                        inputWindow.ShowDialog();
                        return;
                    }
                case nameof(btnOk):
                    {
                        switch (WindowType)
                        {
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
                        }

                        Close();

                        break;
                    }
                case nameof(btnCancel):
                    {
                        switch (WindowType)
                        {
                            case WindowTypes.NewUser:
                                {
                                    break;
                                }
                            case WindowTypes.ResetPassword:
                                {
                                    break;
                                }
                            case WindowTypes.UserInformation:
                                {
                                    break;
                                }
                        }

                        Close();
                        break;
                    }
            }
        }

        #endregion

    }
}
