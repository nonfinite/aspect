using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Properties;
using Aspect.Utility;

using Optional;

using Squirrel;

namespace Aspect.Services
{
    public interface IUpdateService
    {
        Task<Option<Dictionary<ReleaseEntry, string>>> CheckForUpdates();
        void HandleInstallEvents();
        Task<Option<ReleaseEntry>> Update(bool forceUpdate);
    }

    public sealed class UpdateService : IUpdateService
    {
        private readonly Lazy<bool> mIsUpdateable = new Lazy<bool>(() =>
        {
            var assembly = Assembly.GetEntryAssembly();
            var updateDotExe = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "..", "Update.exe");
            var isInstalled = File.Exists(updateDotExe);
            return isInstalled;
        });

        public static IUpdateService Instance { get; } = new UpdateService();

        async Task<Option<Dictionary<ReleaseEntry, string>>> IUpdateService.CheckForUpdates()
        {
            if (!mIsUpdateable.Value)
            {
                return Option.None<Dictionary<ReleaseEntry, string>>();
            }

            using (var mgr = await _CreateUpdateManager())
            {
                if (mgr == null)
                {
                    return Option.None<Dictionary<ReleaseEntry, string>>();
                }

                var updates = await mgr.CheckForUpdate();

                if (updates.FutureReleaseEntry != null &&
                    updates.FutureReleaseEntry.Version != updates.CurrentlyInstalledVersion.Version)
                {
                    return updates.FetchReleaseNotes().Some();
                }

                return Option.None<Dictionary<ReleaseEntry, string>>();
            }
        }

        void IUpdateService.HandleInstallEvents()
        {
            SquirrelAwareApp.HandleEvents(
                async v => await _WithManager(_CreateShortcuts),
                async v => await _WithManager(_CreateShortcuts),
                onAppUninstall: async v => await _WithManager(mgr => mgr.RemoveShortcutForThisExe())
            );

            async Task _WithManager(Action<IUpdateManager> action)
            {
                using (var mgr = await _CreateUpdateManager())
                {
                    if (mgr == null)
                    {
                        return;
                    }

                    action(mgr);
                }
            }
        }

        async Task<Option<ReleaseEntry>> IUpdateService.Update(bool forceUpdate)
        {
            if (!mIsUpdateable.Value)
            {
                return Option.None<ReleaseEntry>();
            }

            if (!forceUpdate && !Settings.Default.UpdateAutomatically)
            {
                return Option.None<ReleaseEntry>();
            }

            using (var mgr = await _CreateUpdateManager())
            {
                if (mgr == null)
                {
                    return Option.None<ReleaseEntry>();
                }

                var results = await mgr.UpdateApp();
                return results.SomeNotNull();
            }
        }

        private static void _CreateShortcuts(IUpdateManager mgr) =>
            mgr.CreateShortcutsForExecutable(
                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                ShortcutLocation.StartMenu | ShortcutLocation.Desktop,
                !Environment.CommandLine.Contains("squirrel-install"), null, null);

        private async Task<IUpdateManager> _CreateUpdateManager()
        {
            var updateUrl = Settings.Default.GitHubUpdateUrl;
            var preRelease = Settings.Default.UpdateToPreRelease;

            this.Log().Information("Creating update manager with {Url} ({PreRelease})", updateUrl, preRelease);

            try
            {
                return await UpdateManager.GitHubUpdateManager(updateUrl, prerelease: preRelease);
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, "Failed to create GitHub update manager");
                return null;
            }
        }
    }
}
