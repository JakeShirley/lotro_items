using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace lotro_items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ItemStats _currentItem = null;
        private Windows.UI.Core.CoreDispatcher _mainThreadDispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;


        public MainPage()
        {
            this.InitializeComponent();

            WikiRequest paintedShell = new WikiRequest("https://lotro-wiki.com/index.php/Item%3AGleaming_Painted_Shell_(Level_99)");
            paintedShell.requestItem().ContinueWith(itemRequest =>
            {
                ItemStats result = itemRequest.Result;
            });

            WikiRequest requestCloak = new WikiRequest("https://lotro-wiki.com/index.php/Item%3AResolute_Cloak_of_Penetration_(Level_99)");
            requestCloak.requestItem().ContinueWith(itemRequest =>
            {
                ItemStats result = itemRequest.Result;
            });
        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            WikiRequest requestCloak = new WikiRequest(txtUrl.Text);
            requestCloak.requestItem().ContinueWith(itemRequest =>
            {
            _currentItem = itemRequest.Result;

                _mainThreadDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txtDebugInfo.Text = _currentItem.PropertyList();
                });
            });
        }
    }
}
