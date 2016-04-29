using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.NetMicroFrameworkService;
using Microsoft.NetMicroFramework.Tools.UsbDebug;
using Microsoft.Practices.ServiceLocation;

namespace MFDeploy.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            #region ViewModels
            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<ConfigNetworkViewModel>();
            SimpleIoc.Default.Register<ConfigUSBViewModel>();
            SimpleIoc.Default.Register<DeployViewModel>();
            SimpleIoc.Default.Register<DeviceCapabilitiesViewModel>();
            #endregion


            #region services
            SimpleIoc.Default.Register<IBusyService, BusyService>();
            SimpleIoc.Default.Register<IMyDialogService, MyDialogService>();

            var usbClient = CreateUSBDebugClient();
            SimpleIoc.Default.Register<INetMFUsbDebugClientService>( () => usbClient);
            #endregion
        }


        #region services configuration
        private static INetMFUsbDebugClientService CreateUSBDebugClient()
        {
            // TODO: check app lifecycle
            var usbDebugClient = new UsbDebugClient(App.Current);            
            return new NetMFUsbDebugClientService(usbDebugClient);
        }
        #endregion
        #region view model properties
        public MainPageViewModel MainPageViewModel { get { return ServiceLocator.Current.GetInstance<MainPageViewModel>(); } }
        public SettingsPageViewModel SettingsPageViewModel { get { return ServiceLocator.Current.GetInstance<SettingsPageViewModel>(); } }
        public ConfigNetworkViewModel ConfigNetworkViewModel { get { return ServiceLocator.Current.GetInstance<ConfigNetworkViewModel>(); } }
        public ConfigUSBViewModel ConfigUSBViewModel { get { return ServiceLocator.Current.GetInstance<ConfigUSBViewModel>(); } }
        public DeployViewModel DeployViewModel { get { return ServiceLocator.Current.GetInstance<DeployViewModel>(); } }
        public DeviceCapabilitiesViewModel DeviceCapabilitiesViewModel { get { return ServiceLocator.Current.GetInstance<DeviceCapabilitiesViewModel>(); } }

        #endregion
    }
}
