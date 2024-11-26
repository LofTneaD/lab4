using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1_1_Click(object sender, RoutedEventArgs e)
        {
            SelectSortWindow selectSortWindow = new SelectSortWindow();
            selectSortWindow.Show();
        }
        private void Button1_2_Click(object sender, RoutedEventArgs e)
        {
            BubbleSortWindow bubbleSortWindow = new BubbleSortWindow();
            bubbleSortWindow.Show();
        }
        private void Button1_3_Click(object sender, RoutedEventArgs e)
        {
            QuickSortWindow quickSortWindow = new QuickSortWindow();
            quickSortWindow.Show();
        }
        private void Button1_4_Click(object sender, RoutedEventArgs e)
        {
            TimSortWindow timSortWindow = new TimSortWindow();
            timSortWindow.Show();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            ExternalSortApp S2 = new ExternalSortApp();
            S2.Show();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}