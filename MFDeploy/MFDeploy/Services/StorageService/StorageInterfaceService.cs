using MFDeploy.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MFDeploy.Services.StorageService
{
    public class StorageInterfaceService : IStorageInterfaceService
    {
        public string DeployFolderToken { get { return "DeployFolderToken"; } }

        public bool IsDeployFolderAvailable { get { return Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(DeployFolderToken); } }

        public async Task<StorageFolder> GetDeployFolder()
        {
            StorageFolder stFolder;
            if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(DeployFolderToken))
                stFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(DeployFolderToken);
            else
                stFolder = null;
            return stFolder;
        }

        public async Task<string> PickDeployFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // save it to access list for future use
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(DeployFolderToken, folder);
                return folder.Path;
            }

            return String.Empty;
        }

        public async Task<IReadOnlyList<StorageFile>> GetDeployFiles()
        {
            if (IsDeployFolderAvailable)
            {
                StorageFolder deployfolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(DeployFolderToken);

                // get files from default folder
                IReadOnlyList<StorageFile> fileList = await deployfolder.GetFilesAsync();

                return fileList;
            }

            return null;
        }
    }
}
