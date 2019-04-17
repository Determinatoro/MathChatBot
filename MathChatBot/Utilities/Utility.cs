﻿using MathChatBot.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MathChatBot.Utilities
{
    public static class Utility
    {

        #region Methods

        /// <summary>
        /// Convert base64 string to bitmap image
        /// </summary>
        /// <param name="base64String">The base64 string</param>
        /// <returns></returns>
        public static BitmapImage Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] binaryData = Convert.FromBase64String(base64String);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(binaryData);
            bi.EndInit();
            return bi;
        }

        /// <summary>
        /// Convert image to base64 string 
        /// </summary>
        /// <param name="filePath">File path to the image</param>
        /// <returns></returns>
        public static string ImageToBase64(string filePath)
        {
            byte[] imageArray = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(imageArray);
        }

        /// <summary>
        /// Copy properties with the same names from one object to another
        /// </summary>
        /// <typeparam name="T">Source type</typeparam>
        /// <typeparam name="U">Dest type</typeparam>
        /// <param name="source">The source object</param>
        /// <param name="dest">The destination object</param>
        public static void CopyPropertiesTo<T, U>(this T source, U dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(U).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    try
                    {
                        var p = destProps.FirstOrDefault(x => x.Name == sourceProp.Name);
                        if (p != null)
                            p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    } catch { }
                }
            }
        }

        /// <summary>
        /// Parse enum from string value
        /// </summary>
        /// <typeparam name="T">Any enum type</typeparam>
        /// <param name="value">String value</param>
        /// <returns>Enum for the string</returns>
        public static T ParseEnum<T>(string value)
        {
            if (!typeof(T).IsEnum || !Enum.IsDefined(typeof(T), value))
                return default(T);
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string RegexReplace(this string source, string pattern, string replacement)
        {
            return Regex.Replace(source, pattern, replacement);
        }

        public static string ReplaceLastOccurence(this string source, string value, string replacement)
        {
            return RegexReplace(source, $"{value}$", replacement);
        }

        public static Dictionary<string, string> GetLocaleResources()
        {
            ResourceManager rm = Properties.Resources.ResourceManager;

            ResourceSet rs = rm.GetResourceSet(CultureInfo.CurrentCulture, true, true);

            Dictionary<string, string> properties = new Dictionary<string, string>();
            foreach (var entry in rs.Cast<DictionaryEntry>())
            {
                properties[entry.Key.ToString()] = entry.Value.ToString();
            }

            return properties;
        }

        /// <summary>
        /// A methpod that replaces words with other words, this function ignores case.
        /// </summary>
        /// <param name="str">The input string that is to be changed</param>
        /// <param name="pattern">The specific word that has to be changed in the string</param>
        /// <param name="replaceStr">The word that replaces the pattern input</param>
        /// <returns>Returns the modified string</returns>
        public static string ReplaceIgnoreCase(this string str, string pattern, string replaceStr)
        {
            return Regex.Replace(str, pattern, replaceStr, RegexOptions.IgnoreCase);
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

        /// <summary>
        /// Run code on UI thread
        /// </summary>
        /// <param name="window">The window who has to run something on the UI thread</param>
        /// <param name="action"></param>
        public static void RunOnUIThread(this Window window, Action action)
        {
            if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
                action(); 
            else
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