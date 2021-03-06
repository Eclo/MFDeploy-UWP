﻿using System;
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
using Template10.Common;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;

namespace MFDeploy.ViewModels
{
    public class MainViewModel : MyViewModelBase
    {
        // messaging tokens
        public const int WRITE_TO_OUTPUT_TOKEN = 1;
        public const int SELECTED_NULL_TOKEN = 2;

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

        private void UsbDebugClient_DeviceEnumerationCompleted(object sender, EventArgs e)
        {
            UsbDebugService.UsbDebugClient.DeviceEnumerationCompleted -= UsbDebugClient_DeviceEnumerationCompleted;            
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
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
                    BusySrv.ShowBusy("Not implemented yet! Why not give it a try??");                    
                    await Task.Delay(2500);
                    await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                    {
                        AvailableDevices = new ObservableCollection<MFDeviceBase>();
                        SelectedDevice = null;
                    });
                    BusySrv.HideBusy();
                    break;

                case TransportType.Usb:
                   
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        BusySrv.ShowBusy(Res.GetString("HC_Searching"));
                        AvailableDevices = new ObservableCollection<MFDeviceBase>(UsbDebugService.UsbDebugClient.MFDevices);
                        UsbDebugService.UsbDebugClient.MFDevices.CollectionChanged += MFDevices_CollectionChanged;
                        // if there's just one, select it
                        SelectedDevice = (AvailableDevices.Count == 1) ? AvailableDevices.First() : null;
                        BusySrv.HideBusy();
                    });
                   
                    break;

                case TransportType.TcpIp:
                    // TODO
                    BusySrv.ShowBusy("Not implemented yet! Why not give it a try??");
                    await Task.Delay(2500);
                    await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                    {
                        AvailableDevices = new ObservableCollection<MFDeviceBase>();
                        SelectedDevice = null;
                    });
                    BusySrv.HideBusy();
                    break;
            }
        }

        private  void MFDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                AvailableDevices = new ObservableCollection<MFDeviceBase>(UsbDebugService.UsbDebugClient.MFDevices);
                // if there's just one, select it
                SelectedDevice = (AvailableDevices.Count == 1) ? AvailableDevices.First() : null;
            });
        }

        public MFDeviceBase SelectedDevice { get; set; }

        public void OnSelectedDeviceChanged()
        {
            SelectedDeviceConnectionResult = PingConnectionResult.None;

            if (SelectedDevice != null)
            {
                SelectedDevice.DebugEngine.SpuriousCharactersReceived -= DebugEngine_SpuriousCharactersReceived;
                SelectedDevice.DebugEngine.SpuriousCharactersReceived += DebugEngine_SpuriousCharactersReceived;
            }
            else
            {
                this.MessengerInstance.Send<NotificationMessage>(new NotificationMessage(""), SELECTED_NULL_TOKEN);
            }
            // try to connect
           SelectedDeviceConnect();           
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

        public void ConnectDisconnect()
        {                        
            if (ConnectionStateResult == ConnectionState.Connected)
            {
                SelectedDeviceDisconnect();
            }
            else
            {
                SelectedDeviceConnect();               
            }
        }

        private async void SelectedDeviceConnect()
        {
            if (SelectedDevice != null)
            {
                await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                {
                    IsBusyHeader = true;
                    ConnectionStateResult = ConnectionState.Connecting;
                });

                bool connectOk = await SelectedDevice.DebugEngine.ConnectAsync(3, 1000);

                await WindowWrapper.Current().Dispatcher.DispatchAsync(() =>
                {
                    ConnectionStateResult = connectOk ? ConnectionState.Connected : ConnectionState.Disconnected;
                    IsBusyHeader = false;
                });
                if (!connectOk)
                {
                    await DialogSrv.ShowMessageAsync(Res.GetString("HC_ConnectionError"));
                }
            }
        }

        private void SelectedDeviceDisconnect()
        {
            if (SelectedDevice != null)
            {
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    IsBusyHeader = true;
                    ConnectionStateResult = ConnectionState.Disconnecting;
                });
                SelectedDevice.DebugEngine.Disconnect();
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    ConnectionStateResult = ConnectionState.Disconnected;
                    IsBusyHeader = false;
                });
            }                      
        }

        #endregion


    }

}
