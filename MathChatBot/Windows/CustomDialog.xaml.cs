using MathChatBot.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace MathChatBot
{

    public enum CustomDialogTypes
    {
        Question,
        Progress,
        Message,
        Error
    }

    public enum CustomDialogQuestionTypes
    {
        YesNo,
        OkCancel
    }

    public enum CustomDialogQuestionResult
    {
        None,
        Yes,
        No,
        Ok,
        Cancel
    }

    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {

        //*************************************************/
        // VARIABLES
        //*************************************************/
        #region Variables

        public static CustomDialog customDialog;

        #endregion

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        private CustomDialogTypes DialogType { get; set; }
        private bool ClosedInCode { get; set; }
        private bool IsShown
        {
            get
            {
                return customDialog.IsLoaded;
            }
        }

        private CustomDialogQuestionTypes QuestionType { get; set; }
        private CustomDialogQuestionResult QuestionResult { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        private CustomDialog(CustomDialogTypes customDialogType)
        {
            InitializeComponent();

            ClosedInCode = false;
            DialogType = customDialogType;

            gridMessageDialog.Visibility = Visibility.Collapsed;
            gridProgressDialog.Visibility = Visibility.Collapsed;
            gridQuestionDialog.Visibility = Visibility.Collapsed;
        }

        private CustomDialog(CustomDialogTypes customDialogTypes, string message, bool isIndeterminate = true, double maximum = 100, RoutedEventHandler clickEvent = null, bool hideCancelButton = false) : this(customDialogTypes)
        {
            message = message.Replace("\\n", "\n");

            switch (customDialogTypes)
            {
                case CustomDialogTypes.Progress:
                    {
                        gridProgressDialog.Visibility = Visibility.Visible;

                        tbProgressMessage.Text = message;

                        pbProgress.IsIndeterminate = isIndeterminate;
                        pbProgress.Value = 0;
                        pbProgress.Maximum = maximum;

                        btnProgressCancel.Click += button_Click;
                        if (clickEvent != null)
                            btnProgressCancel.Click += clickEvent;

                        btnProgressCancel.Visibility = hideCancelButton ? Visibility.Hidden : Visibility.Visible;

                        btnProgressCancel.Content = Properties.Resources.cancel;

                        this.SetupBorderHeader();

                        break;
                    }
                case CustomDialogTypes.Message:
                    {
                        gridMessageDialog.Visibility = Visibility.Visible;

                        tbMessage.Text = message;
                        tbMessage.Loaded += (se, args) =>
                        {
                            tbMessage.HorizontalContentAlignment = tbMessage.LineCount <= 1 ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                        };

                        btnMessageCancel.Click += button_Click;
                        if (clickEvent != null)
                            btnMessageCancel.Click += clickEvent;

                        btnMessageCancel.Content = Properties.Resources.ok;

                        this.KeyDown += window_KeyDown;

                        this.SetupBorderHeader();

                        break;
                    }
            }

            Closing += (s, args) =>
            {
                if (!ClosedInCode)
                    clickEvent?.Invoke(null, null);
            };
        }

        private CustomDialog(string question, CustomDialogQuestionTypes customDialogQuestionTypes) : this(CustomDialogTypes.Question)
        {
            QuestionType = customDialogQuestionTypes;

            this.SetupBorderHeader();
            gridQuestionDialog.Visibility = Visibility.Visible;

            switch (customDialogQuestionTypes)
            {
                case CustomDialogQuestionTypes.YesNo:
                    {
                        tbQuestion.Text = question;

                        btnQuestionOk.Content = Properties.Resources.yes;
                        btnQuestionCancel.Content = Properties.Resources.no;

                        break;
                    }
                case CustomDialogQuestionTypes.OkCancel:
                    {
                        btnQuestionOk.Content = Properties.Resources.ok;
                        btnQuestionCancel.Content = Properties.Resources.cancel;

                        break;
                    }
            }

            btnQuestionOk.Click += button_Click;
            btnQuestionCancel.Click += button_Click;
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Show a message to the user
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="clickEvent">An optional click event if you want to do anything when the user presses "OK"</param>
        public static void Show(string message, RoutedEventHandler clickEvent = null)
        {
            Dismiss();

            Utility.RunOnUIThread(() =>
            {
                customDialog = new CustomDialog(CustomDialogTypes.Message, message, clickEvent: clickEvent);
                customDialog.Show();
            });
        }

        public static CustomDialogQuestionResult ShowQuestion(string question, CustomDialogQuestionTypes customDialogQuestionTypes)
        {
            Dismiss();
            customDialog = new CustomDialog(question, customDialogQuestionTypes);
            customDialog.ShowDialog();
            return customDialog.QuestionResult;
        }

        /// <summary>
        /// Show an indeterminate progress to the user
        /// </summary>
        /// <param name="message">The message to show for the progress</param>
        /// <param name="clickEvent">An optional click event if you want to do anything when the user presses "Cancel"</param>
        /// <param name="hideCancelButton">Flag for hiding the "Cancel" button</param>
        public static void ShowProgress(string message, RoutedEventHandler clickEvent = null, bool hideCancelButton = false)
        {
            Dismiss();
            customDialog = new CustomDialog(CustomDialogTypes.Progress, message, true, clickEvent: clickEvent, hideCancelButton: hideCancelButton);
            customDialog.Show();
        }

        /// <summary>
        /// Show an determinate progress to the user
        /// </summary>
        /// <param name="message">The message to show for the progress</param>
        /// <param name="maximum">The maximum for the progress</param>
        /// <param name="clickEvent">An optional click event if you want to do anything when the user presses "Cancel"</param>
        public static void ShowProgress(string message, double maximum, RoutedEventHandler clickEvent = null)
        {
            Dismiss();
            customDialog = new CustomDialog(CustomDialogTypes.Progress, message, false, maximum, clickEvent);
            customDialog.Show();
        }

        /// <summary>
        /// Set progress value
        /// </summary>
        /// <param name="value">The value</param>
        public static void SetProgress(double value)
        {
            if (customDialog != null && customDialog.IsShown && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value = value;
        }

        /// <summary>
        /// Increment progress with a given value
        /// </summary>
        /// <param name="increment">The increment value</param>
        public static void IncrementProgress(double increment)
        {
            if (customDialog != null && customDialog.IsShown && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value += increment;
        }

        /// <summary>
        /// Dismiss dialog
        /// </summary>
        public static void Dismiss()
        {
            Utility.RunOnUIThread(() =>
            {
                if (customDialog != null && customDialog.IsShown)
                {
                    customDialog.ClosedInCode = true;
                    customDialog.Close();
                }
            });
        }

        public static bool DialogIsShown()
        {
            return customDialog != null && customDialog.IsShown;
        }

        #endregion

        //*************************************************/
        // EVENTS
        //*************************************************/
        #region Events

        // Window - KeyDown
        private void window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    {
                        button_Click(btnMessageCancel, null);
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
                case nameof(btnQuestionOk):
                    {
                        switch (QuestionType)
                        {
                            case CustomDialogQuestionTypes.YesNo:
                                QuestionResult = CustomDialogQuestionResult.Yes;
                                break;
                            case CustomDialogQuestionTypes.OkCancel:
                                QuestionResult = CustomDialogQuestionResult.Ok;
                                break;
                        }

                        break;
                    }
                case nameof(btnQuestionCancel):
                    {
                        switch (QuestionType)
                        {
                            case CustomDialogQuestionTypes.YesNo:
                                QuestionResult = CustomDialogQuestionResult.No;
                                break;
                            case CustomDialogQuestionTypes.OkCancel:
                                QuestionResult = CustomDialogQuestionResult.Cancel;
                                break;
                        }

                        break;
                    }
            }

            Close();
        }

        #endregion

    }
}
