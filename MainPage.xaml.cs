using HtmlAgilityPack;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace lotro_items
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ItemDescription _currentItem = null;
        private Windows.UI.Core.CoreDispatcher _mainThreadDispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        private ConcurrentBag<ItemDescription> _cachedItems = new ConcurrentBag<ItemDescription>();
        private ConcurrentDictionary<string, bool> _visitedPages = new ConcurrentDictionary<string, bool>();

        private async void _updateStats()
        {
            _mainThreadDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtDebugStats.Text = string.Format("URLs Processed: {0}\nItems Proccessed: {1}", _visitedPages.Count, _cachedItems.Count);
            });
        }

        private void _crawlPage(string url)
        {
            if(_visitedPages.ContainsKey(url))
            {
                return;
            }
            else
            {
                _visitedPages.TryAdd(url, true);

                if(_visitedPages.Count % 25 == 0)
                {
                    _updateStats();
                }
            }

            WikiRequest requestCloak = new WikiRequest(url);
            requestCloak.sendRequest().ContinueWith(async body =>
            {
                string bodyText = body.Result;
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.LoadHtml(bodyText);

                List<string> linksCache = new List<string>();
                var links = htmlDoc.DocumentNode.Descendants("a");
                string parentUrl = UrlHelper.GetDomain(requestCloak.URL);

                foreach (var element in links)
                {
                    // Arbitrary wait
                    await Task.Delay(TimeSpan.FromMilliseconds(200));

                    if (element.Attributes.Contains("href"))
                    {
                        string uri = element.Attributes["href"].Value;
                        string domain = UrlHelper.GetDomain(uri);
                        if (domain.Empty())
                        {
                            uri = parentUrl + uri;
                        }
                        else if (domain != parentUrl) // Off-site URL, ignore
                        {
                            continue;
                        }

                        if (uri.Contains("Item:"))
                        {
                            new WikiRequest(uri).requestItem().ContinueWith(itemRequest =>
                            {
                                if (itemRequest.Result != null)
                                {
                                    _cachedItems.Add(itemRequest.Result);
                                }
                            });

                        }
                        _crawlPage(uri);
                    }
                }

            });

        }

        public MainPage()
        {
            this.InitializeComponent();

            /*
            WikiRequest paintedShell = new WikiRequest("https://lotro-wiki.com/index.php/Item%3AGleaming_Painted_Shell_(Level_99)");
            paintedShell.requestItem().ContinueWith(itemRequest =>
            {
                ItemDescription result = itemRequest.Result;
            });

            WikiRequest requestCloak = new WikiRequest("https://lotro-wiki.com/index.php/Item%3AResolute_Cloak_of_Penetration_(Level_99)");
            requestCloak.requestItem().ContinueWith(itemRequest =>
            {
                ItemDescription result = itemRequest.Result;
            });
            */


            _crawlPage("https://lotro-wiki.com/index.php/Category:Items_by_Level");
            
        }

        private void txtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            WikiRequest requestCloak = new WikiRequest(txtUrl.Text);
            requestCloak.requestItem().ContinueWith(itemRequest =>
            {
            _currentItem = itemRequest.Result;

                _mainThreadDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    txtDebugInfo.Text = _currentItem.ToString();
                    imgIcon.Source = new BitmapImage(new Uri(_currentItem.IconURL, UriKind.Absolute));
                    
                });
            });
        }
    }
}
