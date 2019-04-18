using MathChatBot.Models;
using MathChatBot.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
        private BarSeries Series { get; set; }
        private CategoryAxis YAxis { get; set; }
        private LinearAxis XAxis { get; set; }
        private List<User> Users { get; set; }

        private bool IsShowingTopics
        {
            get
            {
                return cbbTopics.SelectedItem != null && cbbTopics.SelectedItem.ToString() == Properties.Resources.all_topics;
            }
        }

        public HelpRequestsWindow()
        {
            InitializeComponent();

            SetupDiagram();
            ShowTopics();

            cbbClasses.ItemsSource = DatabaseUtility.GetEntity().Classes.Select(x => x.Name).ToList();
            var topics = DatabaseUtility.GetEntity().Topics.Select(x => x.Name).ToList();
            topics.Insert(0, Properties.Resources.all_topics);
            cbbTopics.ItemsSource = topics;
            cbbTopics.SelectedIndex = 0;

            cbbClasses.SelectionChanged += control_SelectionChanged;
            cbbTopics.SelectionChanged += control_SelectionChanged;
            dgUsers.SelectionChanged += control_SelectionChanged;
            btnSeeForWholeClass.Click += button_Click;

            this.SetupBorderHeader("Help Requests");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(btnSeeForWholeClass):
                    {
                        if (dgUsers.SelectedValue == null)
                            return;

                        CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait, hideCancelButton: true);

                        new Thread(() =>
                        {
                            var users = (List<User>)dgUsers.ItemsSource;

                            var req = new List<HelpRequest>();
                            users.Select(x =>
                            {
                                req.AddRange(x.HelpRequests);
                                return x.HelpRequests;
                            }).ToList();

                            SetData(req, false);

                            this.RunOnUIThread(() =>
                            {
                                CustomDialog.Dismiss();
                                dgUsers.SelectedIndex = -1;
                            });
                        }).Start();

                        break;
                    }
            }
        }

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
                Position = AxisPosition.Left
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
            };

            model.Series.Add(Series);
        }

        private void ShowTopics()
        {
            YAxis.ItemsSource = DatabaseUtility.GetEntity().GetTopicNames().OrderByDescending(x => x).ToList();
            opHelpRequests.InvalidatePlot();
        }

        private void ShowTerms(string topicName)
        {
            YAxis.ItemsSource = DatabaseUtility.GetEntity().Terms
                .Where(x => x.Topic.Name == topicName)
                .Select(x => x.Name)
                .OrderByDescending(x => x)
                .ToList();
            opHelpRequests.InvalidatePlot();
        }

        private void AddTestData()
        {
            /*var materials = DatabaseUtility.GetEntity().Materials.ToList();
            var materialExamples = DatabaseUtility.GetEntity().MaterialExamples.ToList();

            int i = 0;
            foreach (var material in materials)
            {
                if (material.Term == null)
                    continue;
                DatabaseUtility.GetEntity().HelpRequests.Add(new HelpRequest()
                {
                    User = users[i++ % 20],
                    Material = material
                });
            }

            DatabaseUtility.GetEntity().SaveChanges();*/

        }

        private void SetData(List<HelpRequest> helpRequests, bool filter = true)
        {
            List<User> filteredUsers = (List<User>)dgUsers.ItemsSource;
            var examples = helpRequests.Where(x => x.MaterialExample != null).ToList();
            var materials = helpRequests.Where(x => x.Material != null).ToList();

            bool isShowingTopics = this.RunOnUIThread(() => { return IsShowingTopics; });

            var barItems = new List<BarItem>();

            if (isShowingTopics)
            {
                var topics = DatabaseUtility.GetEntity().Topics.ToList();

                if (filter)
                    filteredUsers = Users.Where(x => x.HelpRequests.Count != 0).ToList();

                var groupedMaterials = materials.GroupBy(x => x.Material.Term.Topic.Name);
                var groupedExamples = examples.GroupBy(x => x.MaterialExample.Material.Term.Topic.Name);

                foreach (var topic in topics)
                {
                    var value = 0;

                    if (groupedMaterials.Any(x => x.Key == topic.Name))
                        value += groupedMaterials.FirstOrDefault(x => x.Key == topic.Name).Count();
                    if (groupedExamples.Any(x => x.Key == topic.Name))
                        value += groupedExamples.FirstOrDefault(x => x.Key == topic.Name).Count();

                    barItems.Add(new BarItem
                    {
                        Value = value
                    });
                }
            }
            else
            {
                var topicName = this.RunOnUIThread(() => { return cbbTopics.SelectedItem.ToString(); });

                if (filter)
                {
                    filteredUsers = Users.Where(x => x.HelpRequests.Any(x2 =>
                    {
                        if (x2.Material != null)
                            return x2.Material.Term.Topic.Name == topicName;
                        else
                            return x2.MaterialExample.Material.Term.Topic.Name == topicName;
                    })).ToList();
                }

                var terms = DatabaseUtility.GetEntity().Terms.Where(x => x.Topic.Name == topicName).ToList();

                var groupedMaterials = materials
                    .Where(x => x.Material.Term.Topic.Name == topicName)
                    .GroupBy(x => x.Material.Term.Name);
                var groupedExamples = examples
                    .Where(x => x.MaterialExample.Material.Term.Topic.Name == topicName)
                    .GroupBy(x => x.MaterialExample.Material.Term.Name);

                foreach (var term in terms)
                {
                    var value = 0;

                    if (groupedMaterials.Any(x => x.Key == term.Name))
                        value += groupedMaterials.FirstOrDefault(x => x.Key == term.Name).Count();
                    if (groupedExamples.Any(x => x.Key == term.Name))
                        value += groupedExamples.FirstOrDefault(x => x.Key == term.Name).Count();

                    barItems.Add(new BarItem
                    {
                        Value = value
                    });
                }
            }

            this.RunOnUIThread(() =>
            {
                dgUsers.ItemsSource = filteredUsers;

                if (barItems.Count != 0)
                {
                    var max = barItems.Max(x => x.Value) + 1;
                    XAxis.Maximum = max == 1 ? 100 : max;
                    XAxis.MaximumRange = max == 1 ? 100 : max;
                }
                Series.ItemsSource = barItems;
                opHelpRequests.InvalidatePlot();
            });
        }

        private void control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = sender as FrameworkElement;

            switch (element.Name)
            {
                case nameof(cbbClasses):
                    {
                        CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait, hideCancelButton: true);
                        var @class = cbbClasses.SelectedItem.ToString();

                        new Thread(() =>
                        {
                            var theClass = DatabaseUtility.GetEntity().Classes.FirstOrDefault(x => x.Name == @class);

                            if (theClass == null)
                                return;

                            Users = theClass.UserClassRelations
                                .Select(x => x.User)
                                .Where(x => x.UserRoleRelations.Any(x2 => x2.Role.RoleType == Role.RoleTypes.Student))
                                .ToList();

                            var req = new List<HelpRequest>();
                            Users.Select(x =>
                            {
                                req.AddRange(x.HelpRequests);
                                return x.HelpRequests;
                            }).ToList();

                            SetData(req);

                            this.RunOnUIThread(() =>
                            {
                                CustomDialog.Dismiss();
                            });
                        }).Start();

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

                        CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait, hideCancelButton: true);

                        new Thread(() =>
                        {
                            var req = new List<HelpRequest>();
                            Users.Select(x =>
                            {
                                req.AddRange(x.HelpRequests);
                                return x.HelpRequests;
                            }).ToList();

                            SetData(req);

                            var selectedUser = this.RunOnUIThread(() =>
                            {
                                dgUsers.SelectedValue = user;
                                return dgUsers.SelectedValue as User;
                            });

                            if (selectedUser != null)
                                SetData(user.HelpRequests.ToList(), false);

                            this.RunOnUIThread(() =>
                            {
                                control_SelectionChanged(dgUsers, null);
                                CustomDialog.Dismiss();
                            });
                        }).Start();

                        break;
                    }
                case nameof(dgUsers):
                    {
                        var user = dgUsers.SelectedValue as User;

                        if (user == null)
                            return;

                        CustomDialog.ShowProgress(Properties.Resources.retrieving_data_please_wait, hideCancelButton: true);

                        new Thread(() =>
                        {
                            SetData(user.HelpRequests.ToList(), false);

                            this.RunOnUIThread(() =>
                            {
                                CustomDialog.Dismiss();
                            });
                        }).Start();

                        break;
                    }
            }
        }

        private void CbWholeClass_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
