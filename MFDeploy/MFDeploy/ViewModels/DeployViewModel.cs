using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFDeploy.Services.BusyService;
using MFDeploy.Services.Dialog;
using MFDeploy.Utilities;
using Microsoft.Practices.ServiceLocation;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using MFDeploy.Models;
using System.Collections.ObjectModel;
using Template10.Controls;
using System.IO;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Threading;
using Template10.Common;
using Microsoft.NetMicroFramework.Tools;

namespace MFDeploy.ViewModels
{
    public class DeployViewModel : MyViewModelBase
    {
        //private instance of Main to get general stuff
        private MainViewModel MainVM { get { return ServiceLocator.Current.GetInstance<MainViewModel>(); } }

        public DeployViewModel(IMyDialogService dlg, IBusyService busy)
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
            await Task.CompletedTask;

            MainVM.PageHeader = Res.GetString("DP_PageHeader");
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

        public ObservableCollection<DeployFile> FilesList { get; set; }

        public bool AnyFileSelected { get; set; }

        public CancellationTokenSource CurrentDeploymentTokenSource { get; set; }

        public event EventHandler FilesListLoaded;

        /// <summary>
        /// Opens file picker and populates files list
        /// </summary>
        public async void OpenDeployFiles()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add("*");
            openPicker.FileTypeFilter.Add(".nmf");
            openPicker.FileTypeFilter.Add(".hex");

            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

            if (files.Count > 0)
            {
                // new list
                FilesList = new ObservableCollection<DeployFile>();

                // get each file and add it to collection
                foreach(StorageFile file in files)
                {
                    // check for allowed extensions
                    if (Path.GetExtension(file.Path).ToLower() == ".sig")
                    {
                        // this type of file will be use latter, not now
                        continue;
                    }
                    else if (Path.GetExtension(file.Path).ToLower() != ".nmf" &&
                        Path.GetExtension(file.Path).ToLower() != ".hex")
                    {
                        // file as different or no extension
                        // allowed files without extension are ER_FLASH, ER_RAM, ER_CONFIG, ER_DAT, ER_ResetVector
                        if (file.DisplayName != "ER_FLASH" && file.DisplayName != "ER_RAM" && file.DisplayName != "ER_CONFIG" &&
                            file.DisplayName != "ER_DAT" && file.DisplayName != "ER_ResetVector")
                        {
                            // file not allowed
                            continue;
                        }
                    }
                    // add new files
                    FilesList.Add(new DeployFile(file));
                }
                // any file
                if(FilesList.Count == 0)
                {
                    var dummy = await DialogSrv.ShowMessageAsync(String.Format("Invalid file(s)"));
                    return;
                }
                FilesListLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public async void DeployFile()
        {
            bool success = false;

            // get only selected files
            IEnumerable<DeployFile> deployList = FilesList.ToArray().Where(s => s.Selected == true);

            // sanity check
            if (deployList.Count() <= 0)
                return;

            List<StorageFile> sigfiles = new List<StorageFile>(deployList.Count());

            // show busy
            BusySrv.ShowBusy();

            try
            {
                foreach (DeployFile file in deployList)
                {
                    // sanity checks
                    if (file.DFile.Path.Trim().Length == 0)
                        continue;
                    if (!file.DFile.IsAvailable)
                    {
                        BusySrv.HideBusy();
                        var dummy = await DialogSrv.ShowMessageAsync(String.Format("file {0} could not be opened", file.DFile.DisplayName));
                        return;
                    }

                    // add to sigFiles list, if exists
                    var sigFile = await GetSignatureFileName(file.DFile);
                    if (sigFile == null)
                    {
                        var dummy = await DialogSrv.ShowMessageAsync(String.Format("file {0}.sig could not be opened", file.DFile.DisplayName));
                        return;
                    }
                    sigfiles.Add(sigFile);
                }

                // the code to deploy file goes here...

                // fazer ping
                bool fMicroBooter = (await MainVM.SelectedDevice.PingAsync() == PingConnectionType.MicroBooter);
                if (fMicroBooter)
                {
                    await MainVM.SelectedDevice.DebugEngine.PauseExecutionAsync();
                }

                List<uint> executionPoints = new List<uint>();
                int cnt = 0;
                foreach (DeployFile file in deployList)
                {
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        CurrentDeploymentTokenSource = new CancellationTokenSource();
                    });
                    CancellationToken cancellationToken = CurrentDeploymentTokenSource.Token;

                    if (Path.GetExtension(file.DFile.Path).ToLower() == ".nmf")
                    {

                        if (!await MainVM.SelectedDevice.DeployUpdateAsync(file.DFile, cancellationToken, 
                            new Progress<ProgressReport>((value) =>
                            {
                                // update busy message according to deployment progress
                                BusySrv.ChangeBusyText(value.Status);
                            })
                        ))
                        {
                            // fail
                            var dummy = await DialogSrv.ShowMessageAsync(String.Format("file {0} could not be deployed", file.DFile.DisplayName));
                            return;
                        }
                    }
                    else
                    {
                        var tpl = await MainVM.SelectedDevice.DeployAsync(file.DFile, sigfiles[cnt++], cancellationToken, 
                            new Progress<ProgressReport>((value) => 
                            {
                                // update busy message according to deployment progress
                                BusySrv.ChangeBusyText(value.Status);
                            })
                        );

                        if (!tpl.Item2)
                        {
                            // fail
                            var dummy = await DialogSrv.ShowMessageAsync(String.Format("file {0} could not be deployed", file.DFile.DisplayName));
                            return;
                        }
                        if (tpl.Item1 != 0)
                        {
                            executionPoints.Add(tpl.Item1);
                        }
                    }
                }

                executionPoints.Add(0);

                if (!fMicroBooter)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (await MainVM.SelectedDevice.DebugEngine.ConnectAsync(1, 500, true))
                        {
                            break;
                        }
                    }
                }

                // update busy message according to deployment progress
                BusySrv.ChangeBusyText("Executing Application");

                foreach (uint addr in executionPoints)
                {
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        CurrentDeploymentTokenSource = new CancellationTokenSource();
                    });
                    CancellationToken cancellationToken = CurrentDeploymentTokenSource.Token;
                    if (await MainVM.SelectedDevice.ExecuteAync(addr, cancellationToken))
                    {
                        break;
                    }
                }


                success = true;
            }
            catch { /* TBD */ }
            finally
            {
                // resume execution
                if (MainVM.SelectedDevice.DebugEngine != null)
                {
                    try
                    {
                        if (MainVM.SelectedDevice.DebugEngine.IsConnected && MainVM.SelectedDevice.DebugEngine.ConnectionSource == Microsoft.SPOT.Debugger.WireProtocol.ConnectionSource.TinyCLR)
                        {
                            await MainVM.SelectedDevice.DebugEngine.ResumeExecutionAsync();
                        }
                    }
                    catch
                    {
                    }
                }

                // end busy
                BusySrv.HideBusy();

                // show result to user
                if (success)
                {
                    await DialogSrv.ShowMessageAsync(deployList.Count() > 1 ? Res.GetString("DP_FilesDeployed") : Res.GetString("DP_FileDeployed"));
                }
                else
                {
                    await DialogSrv.ShowMessageAsync(deployList.Count() > 1 ? Res.GetString("DP_FailToDeployFiles") : Res.GetString("DP_FailToDeployFile"));
                }
            }
        }

        /// <summary>
        /// Gets the corresponding sig file type
        /// </summary>
        /// <param name="file">file to be deployed</param>
        /// <returns>The corresponding sig file, null if not found</returns>
        private async Task<StorageFile> GetSignatureFileName(StorageFile file)
        {
            // get file folder
            var folder = await file.GetParentAsync();
            
            // get .sig file version
            var sigFile = await folder.TryGetItemAsync(Path.GetFileNameWithoutExtension(file.Name) + ".sig") as StorageFile;
            if (sigFile != null)
            {
                // file found, return it
                return sigFile;
            }
            // the .sig file version doesn't exists, return null
            return null;
        }
    }
}
