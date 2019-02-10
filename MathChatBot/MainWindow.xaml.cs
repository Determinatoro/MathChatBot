using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            btnSend.Click += button_Click;
            tbChat.KeyDown += textBox_KeyDown;

            SetupChat();
        }

        private void textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var tb = sender as TextBox;
            switch (tb.Name)
            {
                case nameof(tbChat):
                    {
                        if (e.Key == System.Windows.Input.Key.Enter)
                        {
                            AddChatObject(tb.Text);
                            tb.Text = string.Empty;
                        }
                    }
                    break;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            switch (btn.Name)
            {
                case nameof(btnSend):
                    {


                    }
                    break;
            }
        }

    }
}
