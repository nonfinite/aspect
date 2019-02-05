using System.Windows;

using Aspect.Properties;
using Aspect.UI;
using Aspect.Utility;

using Serilog;

namespace Aspect
{
    public partial class App : Application
    {
        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Trace(outputTemplate: "{SourceContext} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            if (Settings.Default.SettingsUpgradeRequired)
            {
                this.Log().Information("Upgrading settings");

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
