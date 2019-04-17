﻿using MathChatBot.Helpers;
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
        #region Properties

        public User User { get; set; }
        private SplashScreenWindow SplashScreenWindow { get; set; }
        private MathChatBotHelper MathChatBotHelper { get; set; }
        private int ScrollIndex { get; set; }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Hide window until user has logged in
            Hide();

            // Click events
            btnAdminControls.Click += button_Click;
            btnLogOut.Click += button_Click;

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

                    // CollectionChanged events
                    MathChatBotHelper.Messages.CollectionChanged += Messages_CollectionChanged;

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
                if (!roles.Any(x => x.RoleType == Role.RoleTypes.Administrator))
                    btnAdminControls.Visibility = Visibility.Collapsed;
                Show();
            }
            else
                Close();
        }

        #endregion

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
                        var adminControlsWindow = new AdminControlsWindow(User);
                        adminControlsWindow.ShowDialog();
                    }
                    break;
                // Log out
                case nameof(btnLogOut):
                    {
                        ShowLogin();
                    }
                    break;
            }
        }

        // Message Button - Click
        private void messageButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            MessageObject message = (MessageObject)btn.DataContext;

            switch (btn.Name)
            {
                case "btnLeft":
                    {
                        if (message.IsTerm)
                            MathChatBotHelper.SeeExample(message);
                        break;
                    }
                case "btnRight":
                    {

                        break;
                    }
                case "btnMiddle":
                    {
                        if (message.IsTopic)
                            MathChatBotHelper.SeeTerms(message);
                        else if (message.IsExample)
                            MathChatBotHelper.SeeDefinition(message);
                        else if (message.IsTerm)
                            MathChatBotHelper.SeeExample(message);
                        break;
                    }
            }
        }

        // ScrollViewer - OnPreviewMouseWheel
        private void scrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        // MathChatBotHelper.Messages - CollectionChanged
        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ScrollIndex = MathChatBotHelper.Messages.IndexOf(MathChatBotHelper.LastBotMessagesAdded[0]);
            lbChat.Items.Refresh();
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

                        for (int i = ScrollIndex; i < MathChatBotHelper.Messages.Count; i++)
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