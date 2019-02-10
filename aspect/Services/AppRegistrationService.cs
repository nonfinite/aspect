using System;
using System.Linq;

using Aspect.Models;
using Aspect.Utility;

using Orc.FileAssociation;

namespace Aspect.Services
{
    public sealed class AppRegistrationService
    {
        public AppRegistrationService()
        {
            this.Log().Information("Creating app registration service");

            mInfo = new ApplicationInfo(typeof(App).Assembly);
            mInfo.SupportedExtensions.AddRange(
                FileData.SupportedFileExtensions
                    .Select(ext => ext.TrimStart('.')));
        }

        private readonly ApplicationInfo mInfo;
        private readonly IApplicationRegistrationService mRegistrationService = new ApplicationRegistrationService();

        private void _RegisterIfMissing()
        {
            try
            {
                if (!mRegistrationService.IsApplicationRegistered(mInfo))
                {
                    this.Log().Information("Registering application");
                    mRegistrationService.RegisterApplication(mInfo);
                }
                else
                {
                    this.Log().Information("Application already registered");
                }
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, "Failed to register application");
            }
        }

        public void Install()
        {
            this.Log().Information("Creating application registration");
            _RegisterIfMissing();
        }

        public void Uninstall()
        {
            this.Log().Information("Uninstalling app information");
            try
            {
                if (mRegistrationService.IsApplicationRegistered(mInfo))
                {
                    mRegistrationService.UnregisterApplication(mInfo);
                }
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, "Failed to unregister application");
            }
        }

        public void Update()
        {
            this.Log().Information("Updating application registration");
            _RegisterIfMissing();
        }
    }
}
