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
    public sealed class UpdateService
    {
        private readonly Lazy<bool> mIsUpdateable = new Lazy<bool>(() =>
        {
            var assembly = Assembly.GetEntryAssembly();
            var updateDotExe = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? "", "..", "Update.exe");
            var isInstalled = File.Exists(updateDotExe);
            return isInstalled;
        });

        private Task<UpdateManager> _CreateUpdateManager()
        {
            var updateUrl = Settings.Default.GitHubUpdateUrl;
            var preRelease = Settings.Default.UpdateToPreRelease;

            this.Log().Information("Creating update manager with {Url} ({PreRelease})", updateUrl, preRelease);

            return UpdateManager.GitHubUpdateManager(updateUrl, prerelease: preRelease);
        }


        public async Task<Option<Dictionary<ReleaseEntry, string>>> CheckForUpdates()
        {
            if (!mIsUpdateable.Value)
            {
                return Option.None<Dictionary<ReleaseEntry, string>>();
            }

            using (var mgr = await _CreateUpdateManager())
            {
                var updates = await mgr.CheckForUpdate();

                if (updates.FutureReleaseEntry != null &&
                    updates.FutureReleaseEntry.Version != updates.CurrentlyInstalledVersion.Version)
                {
                    return updates.FetchReleaseNotes().Some();
                }

                return Option.None<Dictionary<ReleaseEntry, string>>();
            }
        }

        public async Task<Option<ReleaseEntry>> Update(bool forceUpdate)
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
                var results = await mgr.UpdateApp();
                return results.SomeNotNull();
            }
        }
    }
}
