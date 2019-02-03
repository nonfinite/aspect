using System.Windows;
using Aspect.UI;

namespace Aspect
{
    public partial class App : Application
    {
        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            var viewModel = new MainViewModel();
            MainWindow = new MainWindow {DataContext = viewModel};
            MainWindow.Show();
            await viewModel.Initialize(e.Args);
        }
    }
}
