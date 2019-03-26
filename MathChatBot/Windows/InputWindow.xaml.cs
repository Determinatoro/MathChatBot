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
using static MathChatBot.InputWindow;

namespace MathChatBot
{
    public interface IWindowDataTransfer
    {
        void ReceivedData(WindowTypes windowType, object data);
    }

    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public enum WindowTypes
        {
            NewUser,
            ResetPassword,
            UserInformation
        }

        private WindowTypes WindowType { get; set; }
        public User User { get; set; }
        public List<Role> RolesListForUser { get; set; }
        private IWindowDataTransfer WindowDataTransfer { get; set; }
        private MathChatBotEntities MathChatBotEntities { get; set; }

        public InputWindow(MathChatBotEntities mathChatBotEntities, IWindowDataTransfer windowDataTransfer, User user, WindowTypes windowType = WindowTypes.UserInformation) : this()
        {
            MathChatBotEntities = mathChatBotEntities;
            WindowDataTransfer = windowDataTransfer;
            User = user;
            WindowType = windowType;

            switch (WindowType)
            {
                case WindowTypes.NewUser:
                    break;
                case WindowTypes.ResetPassword:
                    break;
                case WindowTypes.UserInformation:
                    {
                        RolesListForUser = DatabaseUtility.GetRolesListForUser(user.Username);
                        lbRoles.ItemsSource = RolesListForUser;

                        btnOk.Content = Properties.Resources.save;

                        this.SetupBorderHeader(Properties.Resources.user_information);
                        break;
                    }
            }
            
        }

        public InputWindow()
        {
            InitializeComponent();

            btnOk.Click += button_Click;
            btnCancel.Click += button_Click;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Name)
            {
                case nameof(btnOk):
                    {
                        switch (WindowType)
                        {
                            case WindowTypes.NewUser:
                                break;
                            case WindowTypes.ResetPassword:
                                break;
                            case WindowTypes.UserInformation:
                                {
                                    var userRoleRelations = User.UserRoleRelations;

                                    foreach (var role in RolesListForUser)
                                    {
                                        var userRoleRelation = userRoleRelations.Where(x => x.RoleId == role.Id).FirstOrDefault();

                                        if ((role.IsAssigned && userRoleRelation != null) || (!role.IsAssigned && userRoleRelation == null))
                                            continue;

                                        if (role.IsAssigned)
                                        {
                                            User.UserRoleRelations.Add(new UserRoleRelation()
                                            {
                                                UserId = User.Id,
                                                RoleId = role.Id
                                            });
                                        }
                                        else
                                            MathChatBotEntities.Entry(userRoleRelation).State = System.Data.Entity.EntityState.Deleted;
                                    }

                                    if (WindowDataTransfer != null)
                                        WindowDataTransfer.ReceivedData(WindowType, User);

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
            }
        }
    }
}
