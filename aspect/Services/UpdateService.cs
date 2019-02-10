using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Utility;

using NuGet;

using Optional;

using Squirrel;

namespace Aspect.Services
{
    public interface IUpdateService
    {
        Task<Option<ReleaseEntry>> CheckForUpdates();
        Task<SemanticVersion> GetCurrentVersion();
        void HandleInstallEvents(string[] args);
        Task<Option<ReleaseEntry>> Update(bool forceUpdate);
    }

    public sealed class UpdateService : IUpdateService
    {
        public UpdateService()
        {
            Settings.Default.PropertyChanged += _HandleSettingsPropertyChanged;

            mUpdateManager = new LazyEx<Task<IUpdateManager>>(async () =>
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
            });
        }

        private readonly Lazy<bool> mIsUpdateable = new Lazy<bool>(() =>
        {
            var assembly = Assembly.GetEntryAssembly();
            var updateDotExe = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "..", "Update.exe");
            var isInstalled = File.Exists(updateDotExe);
            return isInstalled;
        });

        private readonly LazyEx<Task<IUpdateManager>> mUpdateManager;

        public static IUpdateService Instance { get; } = new UpdateService();

        async Task<Option<ReleaseEntry>> IUpdateService.CheckForUpdates()
        {
            if (!mIsUpdateable.Value)
            {
                this.Log().Information("Skipping update check as app is not updateable");
                return Option.None<ReleaseEntry>();
            }

            this.Log().Information("Checking for updates");
            using (var mgr = await mUpdateManager.Value)
            {
                if (mgr == null)
                {
                    return Option.None<ReleaseEntry>();
                }

                var updates = await mgr.CheckForUpdate();

                if (updates.FutureReleaseEntry != null &&
                    updates.FutureReleaseEntry.Version != updates.CurrentlyInstalledVersion.Version)
                {
                    this.Log().Information("Found new update to {Version}", updates.FutureReleaseEntry.Version);
                    return updates.FutureReleaseEntry.Some();
                }

                return Option.None<ReleaseEntry>();
            }
        }

        public async Task<SemanticVersion> GetCurrentVersion()
        {
            SemanticVersion version;

            using (var mgr = await mUpdateManager.Value)
            {
                version = mgr?.CurrentlyInstalledVersion();
            }

            if (version == null)
            {
                var asmVer = typeof(UpdateService).Assembly.GetName().Version;
                version = new SemanticVersion(asmVer);
            }

            this.Log().Information("Retrieved current version of {Version}", version);

            return version;
        }

        void IUpdateService.HandleInstallEvents(string[] args)
        {
            this.Log().Information("Handling squirrel events");
            SquirrelAwareApp.HandleEvents(
                v =>
                {
                    this.Log().Information("Squirrel event: initial install of {Version}", v);
                    _WithManager(_CreateShortcuts);
                },
                v =>
                {
                    this.Log().Information("Squirrel event: updating to {Version}", v);
                    _WithManager(_CreateShortcuts);
                },
                onAppUninstall: v =>
                {
                    this.Log().Information("Squirrel event: uninstalling {Version}", v);
                    _WithManager(mgr => mgr.RemoveShortcutForThisExe());
                },
                onFirstRun: () => { this.Log().Information("Squirrel event: first run"); },
                arguments: args
            );

            void _WithManager(Action<IUpdateManager> action)
            {
                // The async call will deadlock when run at this early stage of the program unless we explicitly run it in a task.
                var mgr = Task.Run(async () => await mUpdateManager.Value).Result;
                using (mgr)
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
                this.Log().Information("Not updating app as it is not updateable");
                return Option.None<ReleaseEntry>();
            }

            if (!forceUpdate && !Settings.Default.UpdateAutomatically)
            {
                this.Log().Information("Not updating app as it is not configured for automatic updates");
                return Option.None<ReleaseEntry>();
            }

            this.Log().Information("Updating app. Forced: {Forced}", forceUpdate);
            using (var mgr = await mUpdateManager.Value)
            {
                if (mgr == null)
                {
                    return Option.None<ReleaseEntry>();
                }

                var results = await mgr.UpdateApp();

                if (results != null)
                {
                    this.Log().Information("Updated to {Version}", results.Version);
                }
                else
                {
                    this.Log().Information("No new version detected");
                }

                return results.SomeNotNull();
            }
        }

        private static void _CreateShortcuts(IUpdateManager mgr) =>
            mgr.CreateShortcutsForExecutable(
                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                ShortcutLocation.StartMenu,
                !Environment.CommandLine.Contains("squirrel-install"), null, null);

        private void _HandleSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.GitHubUpdateUrl) ||
                e.PropertyName == nameof(Settings.UpdateToPreRelease))
            {
                mUpdateManager.Reset();
            }
        }
    }
}
