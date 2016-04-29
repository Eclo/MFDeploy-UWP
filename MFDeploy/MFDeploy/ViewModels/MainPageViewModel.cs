using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.NetMicroFrameworkService;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace MFDeploy.ViewModels
{
    public class MainPageViewModel : MyViewModelBase
    {
        private INetMFUsbDebugClientService UsbDebugService;

        public MainPageViewModel(IMyDialogService dlg, IBusyService busy, INetMFUsbDebugClientService usbService)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }

            this.DialogSrv = dlg;
            this.BusySrv = busy;
            this.UsbDebugService = usbService;
            AvailableTransportTypes = EnumHelper.ListOf<TransportType>();
        }

        string _Value = "Gas";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }


        public TransportType SelectedTransportType { get; set; } 

        public List<TransportType> AvailableTransportTypes { get; set; }


        public void Reset()
        {
            SelectedTransportType = TransportType.Usb;
            
            
        }


        #region Navigation
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Reset();
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;
           
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

    }
}

