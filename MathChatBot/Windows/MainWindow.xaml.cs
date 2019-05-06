using MathChatBot.Helpers;
using MathChatBot.Models;
using MathChatBot.Objects;
using MathChatBot.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        private User _user;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;

                if (MathChatBotHelper != null)
                    MathChatBotHelper.User = _user;
            }
        }

        private SplashScreenWindow SplashScreenWindow { get; set; }
        private MathChatBotHelper MathChatBotHelper { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            var date = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            // Hide window until user has logged in
            Hide();

            // Click events
            btnAdminControls.Click += button_Click;
            btnLogOut.Click += button_Click;
            btnSeeRequests.Click += button_Click;

            // KeyDown events
            tbChat.KeyDown += textBox_KeyDown;

            // StatusChanged events
            lbChat.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

            // Setup header
            this.SetupBorderHeader(Properties.Resources.main);

            new Thread(() =>
            {
                // It takes a while to set this helper up
                MathChatBotHelper = new MathChatBotHelper();
                this.RunOnUIThread(() =>
                {
                    MathChatBotHelper.WriteWelcome();
                    lbChat.ItemsSource = MathChatBotHelper.Messages;

                    // Close splash screen
                    SplashScreenWindow.Close();
                    // Show login window
                    ShowLogin();
                });
            }).Start();

            // Show splash screen
            SplashScreenWindow = new SplashScreenWindow();
            SplashScreenWindow.ShowDialog();
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Show login window
        /// </summary>
        private void ShowLogin()
        {
            Hide();
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();
            if (result ?? false)
            {
                User = loginWindow.User;
                var roles = DatabaseUtility.GetUserRoles(User.Username);

                if (roles.Any(x => x == Role.RoleTypes.Administrator))
                {
                    btnAdminControls.Visibility = Visibility.Visible;
                    btnSeeRequests.Visibility = Visibility.Visible;
                }
                else if (roles.Any(x => x == Role.RoleTypes.Teacher))
                {
                    btnAdminControls.Visibility = Visibility.Collapsed;
                    btnSeeRequests.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAdminControls.Visibility = Visibility.Collapsed;
                    btnSeeRequests.Visibility = Visibility.Collapsed;
                }

                Show();
                CustomDialog.Dismiss();
            }
            else
                Close();
        }

        #endregion

        //*************************************************/
        // EVENTS
        //*************************************************/
        #region Events

        // TextBox - KeyDown
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            switch (tb.Name)
            {
                case nameof(tbChat):
                    {
                        if (e.Key == Key.Enter)
                        {
                            MathChatBotHelper.WriteMessageToBot(tb.Text);
                            tb.Text = string.Empty;
                        }
                    }
                    break;
            }
        }

        // Button - Click
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                // Admin controls
                case nameof(btnAdminControls):
                    {
                        var adminControlsWindow = new AdminControlsWindow();
                        IsEnabled = false;
                        adminControlsWindow.Closing += (s, a) =>
                        {
                            DatabaseUtility.RefreshEntity();
                            IsEnabled = true;
                        };
                        adminControlsWindow.Show();
                    }
                    break;
                // Log out
                case nameof(btnLogOut):
                    {
                        MathChatBotHelper.WriteMessageToBot(Properties.Resources.clear);
                        ShowLogin();
                    }
                    break;
                // See requests
                case nameof(btnSeeRequests):
                    {
                        HelpRequestsWindow helpRequestsWindow = new HelpRequestsWindow(User);
                        IsEnabled = false;
                        helpRequestsWindow.Closing += (s, a) =>
                        {
                            DatabaseUtility.RefreshEntity();
                            IsEnabled = true;
                        };
                        helpRequestsWindow.Show();
                    }
                    break;
            }
        }

        // Message Button - Click
        private void messageButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            MessageObject message = (MessageObject)btn.DataContext;

            MathChatBotHelper.RunCommand(btn.Content.ToString());
        }

        // ScrollViewer - OnPreviewMouseWheel
        private void scrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        // ListBox.ItemContainerGenerator - StatusChanged
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            var itemContainerGenerator = sender as ItemContainerGenerator;

            if (itemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                try
                {
                    ListBoxItem listBoxItem = lbChat.ItemContainerGenerator.ContainerFromIndex(MathChatBotHelper.Messages.Count - 1) as ListBoxItem;
                    listBoxItem.RemoveRoutedEventHandlers(LoadedEvent);

                    // Loaded event for the ListBoxItem
                    listBoxItem.Loaded += (s, args) =>
                    {
                        double totalItemHeight = 0;

                        var scrollIndex = MathChatBotHelper.Messages.IndexOf(MathChatBotHelper.LastBotMessagesAdded[0]);
                        for (int i = scrollIndex; i < MathChatBotHelper.Messages.Count; i++)
                        {
                            ListBoxItem tempListBoxItem = lbChat.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

                            // Get ListBoxItem sizes
                            Point transform = tempListBoxItem.TransformToVisual((Visual)lbChat.Parent).Transform(new Point());
                            Rect listViewItemBounds = VisualTreeHelper.GetDescendantBounds(tempListBoxItem);
                            listViewItemBounds.Offset(transform.X, transform.Y);

                            totalItemHeight += listViewItemBounds.Height;
                        }

                        // Get total height of the scr
                        var totalHeight = svChat.ActualHeight + svChat.ScrollableHeight;
                        // Get offset for top of ListBoxItem
                        var totalOffset = totalHeight - totalItemHeight;

                        // If the item is smaller than the scroll window just scroll to the bottom
                        if (totalHeight <= svChat.ActualHeight)
                            svChat.ScrollToBottom();
                        else
                            svChat.ScrollToVerticalOffset(totalOffset);
                    };
                }
                catch { }
            }
        }

        #endregion

    }
}
