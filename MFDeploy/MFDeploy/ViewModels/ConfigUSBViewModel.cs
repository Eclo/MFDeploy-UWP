﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;

namespace MFDeploy.ViewModels
{
    public class ConfigUSBViewModel : MyViewModelBase
    {
        public ConfigUSBViewModel(IMyDialogService dlg, IBusyService busy)
        {
            this.DialogSrv = dlg;
            this.BusySrv = busy;
        }
    }
}
