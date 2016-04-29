using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MFDeploy.ViewModels;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MFDeploy.Controls
{
    public sealed partial class HeaderControl : UserControl
    {
        // strongly-typed view models enable x:bind
        public MainPageViewModel ViewModel => this.DataContext as MainPageViewModel;
        public HeaderControl()
        {
            this.InitializeComponent();
        }

     



        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Flyout f = this.TransportTypeButton.Flyout as Flyout;
            //if (f != null)
            //{
            //    f.Hide();
            //}
            
            this.TransportTypeButton.Flyout.SetValue(Views.AttachProp.IsOpenProperty, false);
        }

        private void flyout_Opened(object sender, object e)
        {
            this.TransportTypeButton.Flyout.SetValue(Views.AttachProp.IsOpenProperty, true);
        }
    }
}
