using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Utility;

using NuGet;

using Optional;

using Squirrel;

using Settings = Aspect.Properties.Settings;

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
        private readonly Lazy<bool> mIsUpdateable = new Lazy<bool>(() =>
        {
            var assembly = Assembly.GetEntryAssembly();
            var updateDotExe = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "..", "Update.exe");
            var isInstalled = File.Exists(updateDotExe);
            return isInstalled;
        });

        public static IUpdateService Instance { get; } = new UpdateService();

        async Task<Option<ReleaseEntry>> IUpdateService.CheckForUpdates()
        {
            if (!mIsUpdateable.Value)
            {
                return Option.None<ReleaseEntry>();
            }

            using (var mgr = await _CreateUpdateManager())
            {
                if (mgr == null)
                {
                    return Option.None<ReleaseEntry>();
                }

                var updates = await mgr.CheckForUpdate();

                if (updates.FutureReleaseEntry != null &&
                    updates.FutureReleaseEntry.Version != updates.CurrentlyInstalledVersion.Version)
                {
                    return updates.FutureReleaseEntry.Some();
                }

                return Option.None<ReleaseEntry>();
            }
        }

        public async Task<SemanticVersion> GetCurrentVersion()
        {
            SemanticVersion version;

            using (var mgr = await _CreateUpdateManager())
            {
                version = mgr?.CurrentlyInstalledVersion();
            }

            if (version == null)
            {
                var asmVer = typeof(UpdateService).Assembly.GetName().Version;
                version = new SemanticVersion(asmVer);
            }

            return version;
        }

        void IUpdateService.HandleInstallEvents(string[] args)
        {
            SquirrelAwareApp.HandleEvents(
                v => _WithManager(_CreateShortcuts),
                v => _WithManager(_CreateShortcuts),
                onAppUninstall: v => _WithManager(mgr => mgr.RemoveShortcutForThisExe()),
                arguments: args
            );

            void _WithManager(Action<IUpdateManager> action)
            {
                // The async call will deadlock when run at this early stage of the program unless we explicitly run it in a task.
                var mgr = Task.Run(async () => await _CreateUpdateManager()).Result;
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
                ShortcutLocation.StartMenu,
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
