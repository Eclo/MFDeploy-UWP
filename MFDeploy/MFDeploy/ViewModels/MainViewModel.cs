using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using MFDeploy.Models;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.NetMicroFrameworkService;
using MFDeploy.Services.SettingsServices;
using MFDeploy.Utilities;
using Microsoft.NetMicroFramework.Tools;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger.WireProtocol;
using Windows.UI.Core;

namespace MFDeploy.ViewModels
{
    public class MainViewModel : MyViewModelBase
    {
        // messaging tokens
        public const int WRITE_TO_OUTPUT_TOKEN = 1;

        private IAppSettingsService settingsSrv;
        public MainViewModel(IMyDialogService dlg, IBusyService busy, IAppSettingsService settings)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
            this.settingsSrv = settings;

            AvailableTransportTypes = EnumHelper.ListOf<TransportType>();           
            AvailableDevices = new ObservableCollection<MFDeviceBase>();

            SelectedDevice = null;
            SelectedDeviceConnectionResult = PingConnectionResult.None;
            IsBusyHeader = false;
        }


        public INetMFUsbDebugClientService UsbDebugService { get; set; } = null;
        public void OnUsbDebugServiceChanged()
        {
            if (UsbDebugService != null)
            {
                UsbDebugService.UsbDebugClient.DeviceEnumerationCompleted += UsbDebugClient_DeviceEnumerationCompleted;                
            }
        }

        private async void UsbDebugClient_DeviceEnumerationCompleted(object sender, EventArgs e)
        {
            UsbDebugService.UsbDebugClient.DeviceEnumerationCompleted -= UsbDebugClient_DeviceEnumerationCompleted;
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                // Your UI update code goes here!
                SelectedTransportType = TransportType.Usb;
            });
        }

        public string PageHeader { get; set; }
        public bool IsBusyHeader { get; set; }
      
        public ObservableCollection<MFDeviceBase> AvailableDevices { get; set; }

        private async void UpdateAvailableDevices()
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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        // Your UI update code goes here!
                        AvailableDevices = new ObservableCollection<MFDeviceBase>(UsbDebugService.UsbDebugClient.MFDevices);
                        if (AvailableDevices.Count == 1)
                        {
                            // if there's one, select it
                            SelectedDevice = AvailableDevices.First();
                        }
                    });
                    break;
                case TransportType.TcpIp:
                    // TODO
                    BusySrv.ShowBusy("Searching TcpIp...");
                    BusySrv.HideBusy();
                    break;
            }
        }

        public MFDeviceBase SelectedDevice { get; set; }

        public async void OnSelectedDeviceChanged()
        {
            SelectedDeviceConnectionResult = PingConnectionResult.None;

            if (SelectedDevice != null)
            {
                SelectedDevice.DebugEngine.SpuriousCharactersReceived -= DebugEngine_SpuriousCharactersReceived;
                SelectedDevice.DebugEngine.SpuriousCharactersReceived += DebugEngine_SpuriousCharactersReceived;
            }
            // try to connect
            await SelectedDeviceConnect();           
        }

        private void DebugEngine_SpuriousCharactersReceived(object sender, Microsoft.SPOT.Debugger.StringEventArgs e)
        {
            string textToSend = settingsSrv.AddTimestampToOutput ? $"[{DateTime.Now.ToString()}] {e.EventText}" : e.EventText;
            this.MessengerInstance.Send<NotificationMessage>(new NotificationMessage(textToSend), WRITE_TO_OUTPUT_TOKEN);
        }

        public string SelectedDeviceDisplayContent
        {
            get
            {
                return (SelectedDevice != null) ? SelectedDevice.Description :
                    ( (AvailableDevices.Count > 0) ? Res.GetString("HC_SelectADevice") : Res.GetString("HC_NoDevices") );
            }
        }

        #region Transport
        public List<TransportType> AvailableTransportTypes { get; set; }
        public TransportType SelectedTransportType { get; set; }

        public void OnSelectedTransportTypeChanged()
        {
            UpdateAvailableDevices();
        }
        #endregion

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
                PingConnectionType connection = await SelectedDevice.PingAsync();
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

        #region connect / disconnect
        public ConnectionState ConnectionStateResult { get; set; } = ConnectionState.None;

        public bool Connected { get { return (ConnectionStateResult == ConnectionState.Connected); } }
        public bool Disconnected { get { return (ConnectionStateResult == ConnectionState.Disconnected); } }
        public bool Connecting { get { return (ConnectionStateResult == ConnectionState.Connecting); } }
        public bool Disconnecting { get { return (ConnectionStateResult == ConnectionState.Disconnecting); } }

        public async void ConnectDisconnect()
        {                        
            if (ConnectionStateResult == ConnectionState.Connected)
            {
                SelectedDeviceDisconnect();
            }
            else
            {
                await SelectedDeviceConnect();               
            }
        }

        private async Task SelectedDeviceConnect()
        {
            IsBusyHeader = true;
            ConnectionStateResult = ConnectionState.Connecting;
            bool connectOk = await SelectedDevice.DebugEngine.ConnectAsync(3, 1000);

            ConnectionStateResult = connectOk ?  ConnectionState.Connected : ConnectionState.Disconnected;
            if (!connectOk)
            {
                await DialogSrv.ShowMessageAsync(Res.GetString("HC_ConnectionError"));
            }
            IsBusyHeader = false;
        }

        private void SelectedDeviceDisconnect()
        {
            IsBusyHeader = true;
            ConnectionStateResult = ConnectionState.Disconnecting;
            SelectedDevice.DebugEngine.Disconnect();
            ConnectionStateResult = ConnectionState.Disconnected;
            IsBusyHeader = false;
        }

        #endregion
    }

}
