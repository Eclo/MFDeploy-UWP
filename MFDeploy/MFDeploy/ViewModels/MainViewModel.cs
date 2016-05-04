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
using MFDeploy.Utilities;
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
            AvailableDevices = new ObservableCollection<MFDeviceBase>();
            SelectedTransportType = TransportType.Usb;
            SelectedDevice = null;
            SelectedDeviceConnectionResult = PingConnectionResult.None;
            IsBusyHeader = false;

        }


        public string PageHeader { get; set; }
        public bool IsBusyHeader { get; set; }
      
        #region Transport
        public List<TransportType> AvailableTransportTypes { get; set; }
        public TransportType SelectedTransportType { get; set; }

        public void OnSelectedTransportTypeChanged()
        {
            UpdateAvailableDevices();
        }       
        #endregion


        public ObservableCollection<MFDeviceBase> AvailableDevices { get; set; }

        private void UpdateAvailableDevices()
        {            
            switch (SelectedTransportType)
            {
                case TransportType.Serial:
                    // TODO
                    BusySrv.ShowBusy("Searching Serial...");
                    BusySrv.HideBusy();
                    break;
                case TransportType.Usb:
                    // need to implement type conversion from MFDevices to MFDeviceBase to be able to copy reference,
                    // so changes in UsbDebugService.UsbDebugClient.MFDevices get reflected in AvailableDevices
                    AvailableDevices = new ObservableCollection<MFDeviceBase>(UsbDebugService.UsbDebugClient.MFDevices);
                    break;
                case TransportType.TcpIp:
                    // TODO
                    BusySrv.ShowBusy("Searching TcpIp...");
                    BusySrv.HideBusy();
                    break;

            }
        }

        public MFDeviceBase SelectedDevice { get; set; }

        public bool ConnectAvailable { get; set; }

        public void OnSelectedDeviceChanged()
        {
            // TODO
            SelectedDeviceConnectionResult = PingConnectionResult.None;
            // try to connect

            // set ConnectAvailable

        }

        #region ping
        public PingConnectionResult SelectedDeviceConnectionResult { get; set; }
        public bool ConnectionResultOk { get { return (SelectedDeviceConnectionResult == PingConnectionResult.Ok); } }
        public bool ConnectionResultError { get { return (SelectedDeviceConnectionResult == PingConnectionResult.Error); } }
        public bool Pinging { get { return (SelectedDeviceConnectionResult == PingConnectionResult.Busy); } }

        public async void SelectedDevicePing()
        {
            IsBusyHeader = true;
            
            SelectedDeviceConnectionResult = PingConnectionResult.Busy;
            try
            {
                PingConnectionType connection = await SelectedDevice.Ping();
                SelectedDeviceConnectionResult = (connection != PingConnectionType.NoConnection) ? PingConnectionResult.Ok : PingConnectionResult.Error;
            }
            catch
            {
                SelectedDeviceConnectionResult = PingConnectionResult.Error;
            }
            finally
            {
                IsBusyHeader = false;
            }
        }

        #endregion


    }

}
