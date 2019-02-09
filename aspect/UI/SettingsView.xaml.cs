using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Aspect.Utility;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using Optional;

using Squirrel;

namespace Aspect.UI
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private async void _HandleCheckForUpdatesClick(object sender, RoutedEventArgs e)
        {
            var window = (MetroWindow) Window.GetWindow(this);
            var controller = await window.ShowProgressAsync("Updates", "Checking for updates...");
            controller.SetIndeterminate();

            var result = await mViewModel.CheckForUpdates()
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        this.Log().Error(t.Exception.Flatten(), "Manual update check failed");
                        return Option.None<ReleaseEntry>();
                    }

                    return t.Result;
                });

            await controller.CloseAsync();

            await result.Match(
                releaseNotes => _PromptForUpdate(releaseNotes, window),
                () => window.ShowMessageAsync("Updates", "You are fully up to date!"));
        }

        private async Task _PromptForUpdate(ReleaseEntry release, MetroWindow window)
        {
            var shouldUpdate = await window.ShowMessageAsync(
                "Updates", $"A new version {release.Version} is available!",
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = "Update",
                    NegativeButtonText = "Cancel",
                    DefaultButtonFocus = MessageDialogResult.Negative
                });

            if (shouldUpdate == MessageDialogResult.Affirmative)
            {
                var update = await window.ShowProgressAsync("Updates", "Applying updates...");
                update.SetIndeterminate();

                var updateResult = await mViewModel.Update();

                await update.CloseAsync();
                await updateResult.Match(
                    _ => window.ShowMessageAsync("Updates",
                        "Update has been applied and will take effect next time the app is launched!"),
                    () => Task.CompletedTask);
            }
        }
    }
}
