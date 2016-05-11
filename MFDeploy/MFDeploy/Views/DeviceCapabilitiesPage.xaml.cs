using MFDeploy.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MFDeploy.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeviceCapabilitiesPage : Page
    {
        // strongly-typed view models enable x:bind
        public DeviceCapabilitiesViewModel ViewModel => this.DataContext as DeviceCapabilitiesViewModel;

        public DeviceCapabilitiesPage()
        {
            this.InitializeComponent();
        }
    }
}
