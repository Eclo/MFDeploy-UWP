using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;

namespace MFDeploy.Models
{
    [ImplementPropertyChanged]
    public class DeployFile
    {
        public StorageFile DFile { get; set; }

        public BasicProperties FileProperties { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public ulong FileBaseAddress { get; set; }

        public ulong FileSize { get; set; }

        public string FileTimeStamp { get; set; }

        public bool Selected { get; set; }

//        public DeployFile(string fileName, string filePath, ulong fileBaseAddress, ulong fileSize, string fileTimeStamp, bool selected = true)
        public DeployFile(StorageFile dFile, bool selected = true)
        {
            DFile = dFile;
            Selected = selected;

            // 
            LaunchThreadToGetFileProperties();
        }

        private async void LaunchThreadToGetFileProperties()
        {
            await ThreadPool.RunAsync(async (t) =>
            {
                FileProperties = await DFile.GetBasicPropertiesAsync();
            });
        }
    }
}
