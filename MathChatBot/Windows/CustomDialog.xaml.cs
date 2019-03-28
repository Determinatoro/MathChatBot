using MathChatBot.Utilities;
using System.Windows;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {
        public enum CustomDialogTypes
        {
            Progress,
            Message,
            Error
        }

        private static CustomDialog customDialog;

        public static void Show(string message, RoutedEventHandler clickEvent = null)
        {
            Dismiss();
            customDialog = new CustomDialog(CustomDialogTypes.Message, message, clickEvent: clickEvent);
            customDialog.Show();
        }

        public static void ShowProgress(string message, RoutedEventHandler clickEvent = null)
        {
            Dismiss();
            customDialog = new CustomDialog(CustomDialogTypes.Progress, message, true, clickEvent: clickEvent);
            customDialog.Show();
        }

        public static void ShowProgress(string message, double maximum, RoutedEventHandler clickEvent = null)
        {
            Dismiss();
            customDialog = new CustomDialog(CustomDialogTypes.Progress, message, false, maximum, clickEvent);
            customDialog.Show();
        }

        public static void SetProgress(double value)
        {
            if (customDialog != null && customDialog.IsActive && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value = value;
        }

        public static void IncrementProgress(double increment)
        {
            if (customDialog != null && customDialog.IsActive && customDialog.DialogType == CustomDialogTypes.Progress)
                customDialog.pbProgress.Value += increment;
        }

        public static void Dismiss()
        {
            if (customDialog != null && customDialog.IsActive)
            {
                customDialog.ClosedInCode = true;
                customDialog.Close();
            }
        }

        public CustomDialogTypes DialogType { get; set; }
        public bool ClosedInCode { get; set; }

        private CustomDialog(CustomDialogTypes customDialogTypes, string message, bool isIndeterminate = true, double maximum = 100, RoutedEventHandler clickEvent = null)
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
