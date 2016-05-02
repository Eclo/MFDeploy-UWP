using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.NetMicroFrameworkService;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace MFDeploy.ViewModels
{
    public class MainViewModel : MyViewModelBase
    {
        private INetMFUsbDebugClientService UsbDebugService;

        public MainViewModel(IMyDialogService dlg, IBusyService busy, INetMFUsbDebugClientService usbService)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
            this.UsbDebugService = usbService;
            AvailableTransportTypes = EnumHelper.ListOf<TransportType>();

            ConnectedDevices = new ObservableCollection<MFDeviceBase>();

            SelectedTransportType = TransportType.Usb;

        }
        public string PageHeader { get; set; }

        public TransportType SelectedTransportType { get; set; }

        public void OnSelectedTransportTypeChenged()
        {
            switch (SelectedTransportType)
            {
                case TransportType.Serial:
                    // TODO
                    break;
                case TransportType.Usb:

                    break;
                case TransportType.TcpIp:
                    // TODO
                    break;

            }
        }

        public List<TransportType> AvailableTransportTypes { get; set; }

        public ObservableCollection<MFDeviceBase> ConnectedDevices { get; set; }

        public void Reset()
        {
            ConnectedDevices = new ObservableCollection<MFDeviceBase>();

            SelectedTransportType = TransportType.Usb;
        }

    }

}
