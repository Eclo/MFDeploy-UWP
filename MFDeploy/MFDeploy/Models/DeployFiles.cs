using Microsoft.SPOT.Debugger;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
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

        //public string FileName { get; set; }

        //public string FilePath { get; set; }

        public string FileBaseAddress { get; set; }

        public string FileSize { get; set; }

        public string FileTimeStamp { get; set; }

        public bool Selected { get; set; }

//        public DeployFile(string fileName, string filePath, ulong fileBaseAddress, ulong fileSize, string fileTimeStamp, bool selected = true)
        public DeployFile(StorageFile dFile, bool selected = true)
        {
            DFile = dFile;
            Selected = selected;

            // get file property, we need the file date creation
            LaunchThreadToGetFileProperties();

            // get file memory address and size
            LaunchThreadToGetBaseAddressAndSize(dFile);
        }

        private void LaunchThreadToGetBaseAddressAndSize(StorageFile file)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(async () =>
            {
                var r = await SRecordFile.ParseAsync(file, null);

                List<SRecordFile.Block> lb = r.Item2;
                FileSize = String.Format("0x{0}", lb[0].data.Length.ToString("x4"));
                FileBaseAddress = String.Format("0x{0}", lb[0].address.ToString("x4"));
            });
        }

        private void LaunchThreadToGetFileProperties()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(async () =>
            {
                var prop = await DFile.GetBasicPropertiesAsync();
                FileTimeStamp = prop.ItemDate.ToString("g");
            });
        }
    }
}
