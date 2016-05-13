using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.SettingsServices;
using Microsoft.Practices.ServiceLocation;
using Template10.Services.SettingsService;
using MFDeploy.Services.StorageService;

namespace MFDeploy.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            #region ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
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
            SimpleIoc.Default.Register<IAppSettingsService, AppSettingsService>();
            SimpleIoc.Default.Register<IStorageInterfaceService, StorageInterfaceService>();

            // Template 10
            SimpleIoc.Default.Register<ISettingsHelper, SettingsHelper>();

            #endregion
        }

     
        #region view model properties        
        public MainViewModel MainViewModel { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }
        public MainPageViewModel MainPageViewModel { get { return ServiceLocator.Current.GetInstance<MainPageViewModel>(); } }
        public SettingsPageViewModel SettingsPageViewModel { get { return ServiceLocator.Current.GetInstance<SettingsPageViewModel>(); } }
        public ConfigNetworkViewModel ConfigNetworkViewModel { get { return ServiceLocator.Current.GetInstance<ConfigNetworkViewModel>(); } }
        public ConfigUSBViewModel ConfigUSBViewModel { get { return ServiceLocator.Current.GetInstance<ConfigUSBViewModel>(); } }
        public DeployViewModel DeployViewModel { get { return ServiceLocator.Current.GetInstance<DeployViewModel>(); } }
        public DeviceCapabilitiesViewModel DeviceCapabilitiesViewModel { get { return ServiceLocator.Current.GetInstance<DeviceCapabilitiesViewModel>(); } }

        #endregion
    }
}
