using MathChatBot.Utilities;
using System.Windows;

namespace MathChatBot
{

    public enum CustomDialogTypes
    {
        Progress,
        Message,
        Error
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

        public CustomDialogTypes DialogType { get; set; }
        public bool ClosedInCode { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        private CustomDialog(CustomDialogTypes customDialogTypes, string message, bool isIndeterminate = true, double maximum = 100, RoutedEventHandler clickEvent = null, bool hideCancelButton = false)
        {
            InitializeComponent();

            ClosedInCode = false;

            message = message.Replace("\\n", "\n");

            DialogType = customDialogTypes;

            gridMessageDialog.Visibility = Visibility.Collapsed;
            gridProgressDialog.Visibility = Visibility.Collapsed;

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
            customDialog = new CustomDialog(CustomDialogTypes.Message, message, clickEvent: clickEvent);
            customDialog.Show();
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
            if (customDialog != null && customDialog.IsActive && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value = value;
        }

        /// <summary>
        /// Increment progress with a given value
        /// </summary>
        /// <param name="increment">The increment value</param>
        public static void IncrementProgress(double increment)
        {
            if (customDialog != null && customDialog.IsActive && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value += increment;
        }

        /// <summary>
        /// Dismiss dialog
        /// </summary>
        public static void Dismiss()
        {
            if (customDialog != null && customDialog.IsActive)
            {
                customDialog.ClosedInCode = true;
                customDialog.Close();
            }
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
            Close();
        }

        #endregion

    }
}
