using MathChatBot.Models;
using MathChatBot.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for HelpRequestsWindow.xaml
    /// </summary>
    public partial class HelpRequestsWindow : Window
    {

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        private BarSeries Series { get; set; }
        private CategoryAxis YAxis { get; set; }
        private LinearAxis XAxis { get; set; }
        private List<User> Users { get; set; }
        private User User { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public HelpRequestsWindow(User user) : this()
        {
            var roles = DatabaseUtility.GetUserRoles(user.Username);
            List<Class> classes = null;

            // Get classes
            if (roles.Any(x => x == Role.RoleTypes.Administrator))
                classes = DatabaseUtility.GetEntity().Classes.ToList();
            else if (roles.Any(x => x == Role.RoleTypes.Teacher))
                classes = user.UserClassRelations.Select(x => x.Class).ToList();
            cbbClasses.ItemsSource = classes;
        }

        private HelpRequestsWindow()
        {
            InitializeComponent();

            SetupDiagram();
            ShowTopics();

            // Get topics
            var topics = DatabaseUtility.GetEntity().Topics.Select(x => x.Name).ToList();
            // Insert a selection for all topics
            topics.Insert(0, Properties.Resources.all_topics);
            cbbTopics.ItemsSource = topics;
            cbbTopics.SelectedIndex = 0;

            // SelectionChanged events
            cbbClasses.SelectionChanged += control_SelectionChanged;
            cbbTopics.SelectionChanged += control_SelectionChanged;
            dgUsers.SelectionChanged += control_SelectionChanged;

            // Click events
            btnSeeForWholeClass.Click += button_Click;

            this.SetupBorderHeader(Properties.Resources.help_requests);
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Setup the diagram
        /// </summary>
        private void SetupDiagram()
        {
            var model = new PlotModel();
            opHelpRequests.Model = model;

            // X Axis
            XAxis = new LinearAxis()
            {
                Key = "Requests",
                Title = Properties.Resources.number_of_help_requests,
                Minimum = 0,
                Maximum = 30,
                MinimumRange = 0,
                MaximumRange = 30,
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            // Y Axis
            YAxis = new CategoryAxis()
            {
                Key = "Categories",
                Position = AxisPosition.Left,
                IsZoomEnabled = false
            };

            model.Axes.Add(XAxis);
            model.Axes.Add(YAxis);

            Series = new BarSeries()
            {
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0} ",
                FillColor = OxyColor.FromRgb(0, 36, 86),
                TextColor = OxyColor.FromRgb(255, 255, 255),
                FontSize = 12,
                StrokeColor = OxyColor.FromRgb(32, 32, 32),
                StrokeThickness = 1
            };
            Series.MouseDown += (s, e) =>
            {
                // Get selected topic
                string topicName = cbbTopics.SelectedItem.ToString();
                // Get flag
                bool isShowingAllTopics = topicName == Properties.Resources.all_topics;

                if (isShowingAllTopics)
                {
                    var y = (int)Math.Round((s as BarSeries).InverseTransform(e.Position).Y);
                    var list = YAxis.ItemsSource as List<string>;
                    var item = list[y];
                    cbbTopics.SelectedItem = item;
                }
            };

            model.Series.Add(Series);
        }

        /// <summary>
        /// Show the topic names in the diagram
        /// </summary>
        private void ShowTopics()
        {
            YAxis.ItemsSource = DatabaseUtility
                .GetTopicNames()
                .OrderByDescending(x => x)
                .ToList();
            opHelpRequests.InvalidatePlot();
        }

        /// <summary>
        /// Show term names in the diagram for a given topic
        /// </summary>
        /// <param name="topicName">Name of the topic</param>
        private void ShowTerms(string topicName)
        {
            YAxis.ItemsSource = DatabaseUtility
                .GetTermNames(topicName)
                .OrderByDescending(x => x)
                .ToList();
            opHelpRequests.InvalidatePlot();
        }

        /// <summary>
        /// Set data for the diagram
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="keepUserSelected"></param>
        private void SetData(Class @class = null, bool filter = true, bool keepUserSelected = false)
        {
            CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait, hideCancelButton: true);

            // Get selected topic
            string topicName = cbbTopics.SelectedItem.ToString();

            new Thread(() =>
            {
                try
                {
                    // Get all the students in the class
                    if (@class != null)
                        Users = DatabaseUtility.GetUsersInClass(@class, Role.RoleTypes.Student);

                    // If there are no users in the class do not go any further
                    if (Users.Count == 0)
                        return;

                    List<User> users = dgUsers.ItemsSource as List<User>;
                    User selectedUser = null;
                    List<BarItem> barItems = new List<BarItem>();
                    List<HelpRequest> helpRequests = new List<HelpRequest>();

                    if (keepUserSelected)
                        selectedUser = this.RunOnUIThread(() => { return dgUsers.SelectedValue as User; });

                    // Get flag
                    bool isShowingAllTopics = topicName == Properties.Resources.all_topics;

                    if (isShowingAllTopics)
                    {
                        // Only show users who has made a help request
                        if (filter)
                            users = Users
                                .Where(x => x.HelpRequests.Count != 0)
                                .OrderBy(x => x.Name)
                                .ToList();
                    }
                    else
                    {
                        // Only show users who has made a help request for a term under the specific topic
                        if (filter)
                        {
                            users = Users
                                .Where(x => x.HelpRequests.Any(x2 =>
                                {
                                    if (x2.Material != null)
                                        return x2.Material.Term.Topic.Name == topicName;
                                    else
                                        return x2.MaterialExample.Material.Term.Topic.Name == topicName;
                                }))
                                .OrderBy(x => x.Name)
                                .ToList();
                        }
                    }

                    // Get help requests
                    if (keepUserSelected &&
                            selectedUser != null &&
                            users.Any(x => x == selectedUser)
                            )
                    {
                        helpRequests.AddRange(selectedUser.HelpRequests);
                    }
                    else
                    {
                        users.Select(x =>
                        {
                            helpRequests.AddRange(x.HelpRequests);
                            return x.HelpRequests;
                        }).ToList();
                    }

                    // Help requests for term material examples
                    var helpRequestsForExamples = helpRequests.Where(x => x.MaterialExample != null).ToList();
                    // Help requests for term materials
                    var helpRequestsForMaterials = helpRequests.Where(x => x.Material != null).ToList();

                    if (isShowingAllTopics)
                    {
                        // Get topics from database
                        var topics = DatabaseUtility.GetEntity().Topics.ToList();

                        // Group help requests by topics
                        var groupedMaterials = helpRequestsForMaterials.GroupBy(x => x.Material.Term.Topic.Name);
                        var groupedExamples = helpRequestsForExamples.GroupBy(x => x.MaterialExample.Material.Term.Topic.Name);

                        foreach (var topic in topics)
                        {
                            var value = 0;

                            // Get number of help requests for each topic
                            if (groupedMaterials.Any(x => x.Key == topic.Name))
                                value += groupedMaterials.FirstOrDefault(x => x.Key == topic.Name).Count();
                            if (groupedExamples.Any(x => x.Key == topic.Name))
                                value += groupedExamples.FirstOrDefault(x => x.Key == topic.Name).Count();

                            barItems.Add(new BarItem
                            {
                                Value = value,
                                CategoryIndex = (YAxis.ItemsSource as List<string>).IndexOf(topic.Name)
                            });
                        }
                    }
                    else
                    {
                        // Get all terms for the specific topic
                        var terms = DatabaseUtility.GetEntity().Terms.Where(x => x.Topic.Name == topicName).ToList();

                        // Group help requests for materials
                        var groupedMaterials = helpRequestsForMaterials
                            .Where(x => x.Material.Term.Topic.Name == topicName)
                            .GroupBy(x => x.Material.Term.Name);
                        // Group help reuqests for examples
                        var groupedExamples = helpRequestsForExamples
                            .Where(x => x.MaterialExample.Material.Term.Topic.Name == topicName)
                            .GroupBy(x => x.MaterialExample.Material.Term.Name);

                        foreach (var term in terms)
                        {
                            var value = 0;

                            // Get number of help requests for each term
                            if (groupedMaterials.Any(x => x.Key == term.Name))
                                value += groupedMaterials.FirstOrDefault(x => x.Key == term.Name).Count();
                            if (groupedExamples.Any(x => x.Key == term.Name))
                                value += groupedExamples.FirstOrDefault(x => x.Key == term.Name).Count();

                            barItems.Add(new BarItem
                            {
                                Value = value,
                                CategoryIndex = (YAxis.ItemsSource as List<string>).IndexOf(term.Name)
                            });
                        }
                    }

                    this.RunOnUIThread(() =>
                    {
                        // Show users
                        dgUsers.ItemsSource = users;
                        // Set selection
                        dgUsers.SelectedValue = selectedUser;

                        // Set diagram items
                        if (barItems.Count != 0)
                        {
                            var max = barItems.Max(x => x.Value) + 1;
                            XAxis.Maximum = max == 1 ? 100 : max;
                            XAxis.MaximumRange = max == 1 ? 100 : max;
                        }
                        Series.ItemsSource = barItems;
                        opHelpRequests.InvalidatePlot();

                        CustomDialog.Dismiss();
                    });
                }
                catch (Exception mes)
                {
                    this.RunOnUIThread(() =>
                    {
                        if (mes.Message == string.Empty)
                            CustomDialog.Dismiss();
                        else
                            CustomDialog.Show(mes.Message);
                    });
                }
            }).Start();
        }

        #endregion

        //*************************************************/
        // EVENTS
        //*************************************************/
        #region Events

        // Button - Click
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(btnSeeForWholeClass):
                    {
                        if (dgUsers.SelectedValue == null)
                            return;

                        SetData(filter: false);

                        break;
                    }
            }
        }

        // ComboBox, DataGrid - SelectionChanged
        private void control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(cbbClasses):
                    {
                        var @class = cbbClasses.SelectedItem as Class;
                        if (@class == null)
                            return;

                        SetData(@class: @class);

                        break;
                    }
                case nameof(cbbTopics):
                    {
                        var user = dgUsers.SelectedValue as User;
                        var topic = cbbTopics.SelectedItem.ToString();

                        if (topic == Properties.Resources.all_topics)
                            ShowTopics();
                        else
                            ShowTerms(topic);

                        if (cbbClasses.SelectedItem == null)
                            return;

                        SetData(keepUserSelected: true);

                        break;
                    }
                case nameof(dgUsers):
                    {
                        var user = dgUsers.SelectedValue as User;

                        if (user == null)
                            return;

                        SetData(filter: false, keepUserSelected: true);

                        break;
                    }
            }
        }

        #endregion

    }
}
