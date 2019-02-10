using System;
using System.IO;
using System.Windows;

using Aspect.Services;
using Aspect.UI;
using Aspect.Utility;

using Serilog;
using Serilog.Events;

using Splat;

using ILogger = Splat.ILogger;

namespace Aspect
{
    public partial class App : Application
    {
        private static void _ConfigureLogging()
        {
            const long BYTES_PER_MB = 1024 * 1024;

            var logFile = Environment.GetEnvironmentVariable("ASPECT_LOG_FILE");
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location) ?? "", "log.txt");
            }

            if (!Enum.TryParse(Environment.GetEnvironmentVariable("ASPECT_LOG_LEVEL") ?? "", true,
                out LogEventLevel fileLogLevel))
            {
                fileLogLevel = LogEventLevel.Warning;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Trace(outputTemplate: "[{Level:u3}] {SourceContext} => {Message}{NewLine}{Exception}")
                .WriteTo.File(logFile, fileLogLevel, rollOnFileSizeLimit: true, fileSizeLimitBytes: 20 * BYTES_PER_MB,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} => {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Locator.CurrentMutable.RegisterConstant(new SplatLoggerProxy(), typeof(ILogger));
        }

        private void _HandleExit(object sender, ExitEventArgs e) => Log.CloseAndFlush();

        private async void _HandleStartup(object sender, StartupEventArgs e)
        {
            _ConfigureLogging();

            UpdateService.Instance.HandleInstallEvents(e.Args);

            var window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();
            await window.ViewModel.Initialize(e.Args);
        }
    }
}
