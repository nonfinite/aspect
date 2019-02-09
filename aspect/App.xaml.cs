using System.Threading.Tasks;
using System.Windows;

using Aspect.Properties;
using Aspect.Services;
using Aspect.UI;
using Aspect.Utility;

using Serilog;

using Squirrel;

namespace Aspect
{
    public partial class App : Application
    {
        private void _HandleExit(object sender, ExitEventArgs e) => Log.CloseAndFlush();

        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Trace(outputTemplate: "[{Level}] {SourceContext}\r\n=> {Message}{NewLine}{Exception}")
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
