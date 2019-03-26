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

        public static void Show(string message, CustomDialogTypes customDialogTypes = CustomDialogTypes.Message, RoutedEventHandler clickEvent = null)
        {
            Dismiss();
            customDialog = new CustomDialog(customDialogTypes, message, clickEvent);
            customDialog.Show();
        }

        public static void Dismiss()
        {
            if (customDialog != null && customDialog.IsActive)
                customDialog.Close();
        }

        private CustomDialog(CustomDialogTypes customDialogTypes, string message, RoutedEventHandler clickEvent = null)
        {
            InitializeComponent();

            gridMessageDialog.Visibility = Visibility.Collapsed;
            gridProgressDialog.Visibility = Visibility.Collapsed;

            switch (customDialogTypes)
            {
                case CustomDialogTypes.Progress:
                    {
                        gridProgressDialog.Visibility = Visibility.Visible;

                        tbProgressMessage.Text = message;
                        pbProgress.IsIndeterminate = true;
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
                        btnMessageCancel.Click += button_Click;
                        if (clickEvent != null)
                            btnMessageCancel.Click += clickEvent;

                        btnMessageCancel.Content = Properties.Resources.ok;

                        this.SetupBorderHeader();

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
