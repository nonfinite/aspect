using System.Windows;

using Aspect.UI;

namespace Aspect
{
    public partial class App : Application
    {
        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();
            await window.ViewModel.Initialize(e.Args);
        }
    }
}
