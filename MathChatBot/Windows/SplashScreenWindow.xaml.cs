using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MathChatBot
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();

            lblVersion.Content = string.Format(Properties.Resources.version, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            
            Storyboard sb = (Storyboard)this.imgLogo.FindResource("spin");
            sb.Begin();
        }
    }
}
