using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SPOT.Debugger;

namespace MFDeploy.Services.NetMicroFrameworkService
{
    public class NetMFUsbDebugClientService : INetMFUsbDebugClientService
    {
        public PortBase UsbDebugClient { get; private set; }

        public NetMFUsbDebugClientService(PortBase client)
        {
            this.UsbDebugClient = client;
        }

    }
}
