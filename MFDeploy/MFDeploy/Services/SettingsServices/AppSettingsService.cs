using System;
using Template10.Services.SettingsService;

namespace MFDeploy.Services.SettingsServices
{
    public class AppSettingsService : IAppSettingsService
    {
        private ISettingsHelper settingsService;
        public AppSettingsService(ISettingsHelper _settingsService)
        {
            settingsService = _settingsService;
        }


        public bool AddTimestampToOutput
        {
            get { return settingsService.Read<bool>(nameof(AddTimestampToOutput), false, SettingsStrategies.Roam); }
            set { settingsService.Write(nameof(AddTimestampToOutput), value, SettingsStrategies.Roam); }
        }

    }
}

