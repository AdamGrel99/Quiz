using System.Configuration;
using System.Data;
using System.Windows;
using QuizApp.Repository;

namespace QuizApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var repository = new Repository.Repository();
            repository.InitializeDatabase();
        }
    }
}
