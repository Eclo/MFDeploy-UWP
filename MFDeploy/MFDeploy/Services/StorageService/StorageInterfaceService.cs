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
            try
            {
                if (IsDeployFolderAvailable)
                {
                    StorageFolder deployfolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(DeployFolderToken);

                    // dummy filter 
                    List<string> fileTypeFilter = new List<string>();
                    fileTypeFilter.Add("*");

                    Windows.Storage.Search.QueryOptions queryOptions = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter);
                    Windows.Storage.Search.StorageFileQueryResult queryResult = deployfolder.CreateFileQueryWithOptions(queryOptions);

                    // get files from default folder
                    IReadOnlyList<StorageFile> fileList = await queryResult.GetFilesAsync();

                    return fileList;
                }

                //Messenger.Default.Send(new NotificationMessage(""), GlobalNotificationMessages.UpdateProgramPageMissionTemplateListToken);
                return null;
            }
            catch (Exception ex)
            {
                // notify user
                //Messenger.Default.Send(new NotificationMessage(ex.Message), GlobalNotificationMessages.ShowErrorWarningToken);
                ////send telemetry
                //var properties = new Dictionary<string, string> { { "MissionTemplates", "LocalStorageInterface.GetMissionTemplates" } };
                return null;
            }
        }
    }
}
