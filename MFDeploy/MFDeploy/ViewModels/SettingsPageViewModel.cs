using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Services.SettingsServices;
using MFDeploy.Utilities;
using Microsoft.Practices.ServiceLocation;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Services.SettingsService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using MFDeploy.Services.StorageService;

namespace MFDeploy.ViewModels
{
    public class SettingsPageViewModel : MyViewModelBase
    {
        //private instance of Main to get general stuff
        private MainViewModel MainVM { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }

        public SettingsPageViewModel(IMyDialogService dlg, IBusyService busy, IAppSettingsService _appSettings, IStorageInterfaceService _storageInterfaceService)
        {
            SettingsPartViewModel  = new SettingsPartViewModel(dlg, busy, _appSettings);
            AboutPartViewModel = new AboutPartViewModel(dlg, busy);
            StorageInterface = _storageInterfaceService;
        }
        public SettingsPartViewModel SettingsPartViewModel { get; }
        public AboutPartViewModel AboutPartViewModel { get; }

        #region Navigation
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {

            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;

            MainVM.PageHeader = Res.GetString("ST_PageHeader");

            LoadDeployFolder();
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        public string DeployFolderPath { get; set; }

        private async void LoadDeployFolder()
        {
            StorageFolder folder = await StorageInterface.GetDeployFolder();

            if (folder != null)
            {
                DeployFolderPath = folder.Path;
            }
            else
                DeployFolderPath = Res.GetString("ST_CurrentFolderPath");
        }

        public async void PickDeployFolder()
        {
            string folderPath = await StorageInterface.PickDeployFolder();
            if(folderPath != String.Empty)
            {
                DeployFolderPath = folderPath;
            }
        }
    }

    public class SettingsPartViewModel : MyViewModelBase
    {
        IAppSettingsService _settings;

        public SettingsPartViewModel(IMyDialogService dlg, IBusyService busy, IAppSettingsService _appSettings)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
            this._settings = _appSettings;
        }

        public bool AddTimestampToOutputButton
        {
            get { return _settings.AddTimestampToOutput; }
            set { _settings.AddTimestampToOutput = value; }
        }

    }

    public class AboutPartViewModel : MyViewModelBase
    {
        public AboutPartViewModel(IMyDialogService dlg, IBusyService busy)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
        }

        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public Uri RateMe => new Uri("http://aka.ms/template10");
    }

    
}

