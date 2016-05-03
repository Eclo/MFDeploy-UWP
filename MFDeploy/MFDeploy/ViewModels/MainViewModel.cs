using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFDeploy.Models;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.NetMicroFrameworkService;
using Microsoft.NetMicroFramework.Tools;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger.WireProtocol;

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

        #region Transport
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
        #endregion


        public ObservableCollection<MFDeviceBase> ConnectedDevices { get; set; }

        public MFDeviceBase SelectedDevice { get; set; }

        public void OnSelectedDeviceChanged()
        {
            // TODO
            SelectedDeviceConnectionResult = PingConnectionResult.None;
        }

        public PingConnectionResult SelectedDeviceConnectionResult { get; set; }
        public bool ConnectionResultOk { get { return (SelectedDeviceConnectionResult == PingConnectionResult.Ok); } }
        public bool ConnectionResultError { get { return (SelectedDeviceConnectionResult == PingConnectionResult.Error); } }

        public async void SelectedDevicePing()
        {
            // reset this before pinging to force animation to run even if property doesn't change
            SelectedDeviceConnectionResult = PingConnectionResult.None;
            try
            {
                PingConnectionType connection = await SelectedDevice.Ping();
                SelectedDeviceConnectionResult = (connection != PingConnectionType.NoConnection) ? PingConnectionResult.Ok : PingConnectionResult.Error;                
            }
            catch
            {
                SelectedDeviceConnectionResult = PingConnectionResult.Error;
            }
            
        }

        public void Reset()
        {
            SelectedDevice = null;
            SelectedDeviceConnectionResult = PingConnectionResult.None;            
            ConnectedDevices = new ObservableCollection<MFDeviceBase>();            
            SelectedTransportType = TransportType.Usb;
        }

    }

}
