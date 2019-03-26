using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MathChatBot.Utilities
{
    public static class Utility
    {

        #region Methods

        /// <summary>
        /// Parse enum from string value
        /// </summary>
        /// <typeparam name="T">Any enum type</typeparam>
        /// <param name="value">String value</param>
        /// <returns>Enum for the string</returns>
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Get specific event handlers
        /// <summary>
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        public static void RemoveRoutedEventHandlers(this UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            if (eventHandlersStore == null) return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
                element.RemoveHandler(routedEvent, routedEventHandler.Handler);
        }

        public static void RunOnUIThread(this Window window, Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Setup border header
        /// </summary>
        /// <param name="window"></param>
        /// <param name="title"></param>
        public static void SetupBorderHeader(this Window window, string title = "MathChatBot")
        {
            if (title != "MathChatBot")
                title = string.Format(Properties.Resources.application_title, title);

            // Content controls
            var lblTitle = (Label)window.FindName("lblTitle");
            var borderHeader = (Border)window.FindName("borderHeader");
            var btnClose = (Button)window.FindName("btnClose");
            var btnMinimize = (Button)window.FindName("btnMinimize");

            // Remove existing events
            btnClose.RemoveRoutedEventHandlers(ButtonBase.ClickEvent);
            btnMinimize.RemoveRoutedEventHandlers(ButtonBase.ClickEvent);
            borderHeader.RemoveRoutedEventHandlers(UIElement.MouseLeftButtonDownEvent);

            // Events
            btnClose.Click += button_Click;
            btnMinimize.Click += button_Click;
            borderHeader.MouseLeftButtonDown += BorderHeader_MouseDown;

            // Layout
            lblTitle.Content = title;
        }

        /// <summary>
        /// Get parent window from content control
        /// </summary>
        /// <param name="child">The child content control</param>
        /// <returns>The parent window</returns>
        public static Window GetParentWindow(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            Window parent = parentObject as Window;
            if (parent != null)
                return parent;
            else
                return GetParentWindow(parentObject);
        }

        #endregion

        #region Events

        /// <summary>
        /// Mouse down event for border header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void BorderHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var window = GetParentWindow((DependencyObject)sender);
            window.DragMove();
        }

        private static void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var window = GetParentWindow(btn);

            switch (btn.Name)
            {
                case "btnClose":
                    {
                        window.Close();
                        break;
                    }
                case "btnMinimize":
                    {
                        window.WindowState = WindowState.Minimized;
                        break;
                    }
            }
        }

        #endregion

    }
}
