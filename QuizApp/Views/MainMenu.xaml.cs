using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuizApp.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void Rozwiaz_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new QuizView());
        }

        private void Stworz_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateQuizView());
        }
    }
}
