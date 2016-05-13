using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SPOT.Debugger;

namespace MFDeploy.Services.NetMicroFrameworkService
{
    public interface INetMFUsbDebugClientService : INetMFDebugClientBaseService
    {
        PortBase UsbDebugClient { get; }

    }
}
