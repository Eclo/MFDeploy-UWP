using System;
using System.Collections.Generic;
using System.Linq;

namespace MFDeploy.Services.SettingsServices
{
    public interface IAppSettingsService
    {
        bool AddTimestampToOutput { get; set; }
    }
}
