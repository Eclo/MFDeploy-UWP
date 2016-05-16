﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Utilities;
using MFDeploy.Views.Config;
using Microsoft.Practices.ServiceLocation;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace MFDeploy.ViewModels
{
    public class ConfigUSBViewModel : MyViewModelBase
    {
        //private instance of Main to get general stuff
        private MainViewModel MainVM { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }

        public ConfigUSBViewModel(IMyDialogService dlg, IBusyService busy)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
        }

        #region Navigation
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {

            if (suspensionState.Any())
            {
                //Value = suspensionState[nameof(Value)]?.ToString();
            }
            MessengerInstance.Register<NotificationMessage>(this, MainViewModel.SELECTED_NULL_TOKEN, (message) => SelectedIsNullHandler());
            await Task.CompletedTask;

            MainVM.PageHeader = Res.GetString("CU_PageHeader");
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(Value)] = Value;
            }
            MessengerInstance.Unregister(this);
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion
        private void SelectedIsNullHandler()
        {
            this.NavigationService.Navigate(Pages.MainPage);
        }
    }
}
