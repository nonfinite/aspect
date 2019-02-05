using System.Windows;

using Aspect.Properties;
using Aspect.UI;

namespace Aspect
{
    public partial class App : Application
    {
        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            if (Settings.Default.SettingsUpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgradeRequired = false;
                Settings.Default.Save();
            }

            var window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();
            await window.ViewModel.Initialize(e.Args);
        }
    }
}
