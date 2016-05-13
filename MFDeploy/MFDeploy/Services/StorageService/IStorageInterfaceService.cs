using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MFDeploy.Services.StorageService
{
    public interface IStorageInterfaceService
    {
        string DeployFolderToken { get; }
        bool IsDeployFolderAvailable { get; }
        Task<StorageFolder> GetDeployFolder();
        Task<string> PickDeployFolder();
        Task<IReadOnlyList<StorageFile>> GetDeployFiles();
    }
}
