using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace HoloPi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultsPage : Page
    {
        JsonArray ja;

        // selected ListViewItem index
        int index;

        public ResultsPage()
        {
            this.InitializeComponent();
        }

        private void AppBarBackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DetectionPage));
        }

        private void CommandBar_Opening(object sender, object e)
        {
            CommandBar cb = sender as CommandBar;
            if (cb != null) cb.Background.Opacity = 1.0;
        }

        private void CommandBar_Closing(object sender, object e)
        {
            CommandBar cb = sender as CommandBar;
            if (cb != null) cb.Background.Opacity = 0.5;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is object && (e.Parameter != null))
            {
                ja = e.Parameter as JsonArray;
                AddItemToList();    
            }
            InitializePage();
        }

        private void InitializePage()
        {
            if (ja.Count > 0)
            {
                ItemName.Text = ja[0].GetObject().GetNamedString("ItemName");
                ItemDescription.Text = ja[0].GetObject().GetNamedString("ItemDescription");
                SetImage(ja[0].GetObject());
            }
            else
            {
                ItemName.Text = "";
                ItemDescription.Text = "";
                ItemImage.Source = null;
            }
        }

        private void AddItemToList()
        {
            int itemCounts = ja.Count;

            for (int i = 0; i < itemCounts; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Content = (i + 1) + "." + ja[i].GetObject().GetNamedString("ItemName");
                item.FontSize = 20;
                item.Tapped += Item_Tapped;
                ItemList.Items.Add(item);
            }
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            index = int.Parse(item.Content.ToString().Split('.')[0]) - 1;
            var jo = ja[index].GetObject();

            // set the image
            SetImage(jo);

            // set the description
            ItemDescription.Text = jo.GetNamedString("ItemDescription");

            // set the title
            ItemName.Text = jo.GetNamedString("ItemName");
        }

        private async void SetImage(JsonObject jo)
        {
            var bytes = Convert.FromBase64String(jo.GetNamedString("ItemImage"));
            var Imagebuf = bytes.AsBuffer();
            var Imagestream = Imagebuf.AsStream();
            BitmapImage bmpImage = new BitmapImage();
            await bmpImage.SetSourceAsync(Imagestream.AsRandomAccessStream());
            ItemImage.Source = bmpImage;
        }

        private async void AppBarShareButton_Click(object sender, RoutedEventArgs e)
        {
            if (ja.Count > 0)
            {
                this.Frame.IsEnabled = false;
                pring.IsActive = true;
                pring.Visibility = Visibility.Visible;

                HttpClient httpClient = new HttpClient();
                JsonObject jo = new JsonObject();
                jo.SetNamedValue("sharing", JsonValue.Parse(ja.ToString()));
                HttpStringContent content = new HttpStringContent(jo.ToString());
                
                content.Headers.ContentType = 
                    new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/json");

                HttpResponseMessage response = new HttpResponseMessage();
                response = await httpClient.PostAsync(new Uri(
                                        "http://" + DetectionPage.GetIP() + ":5000/share"), content);
                string resp = await response.Content.ReadAsStringAsync();
                httpClient.Dispose();

                this.Frame.IsEnabled = true;
                pring.IsActive = false;
                pring.Visibility = Visibility.Collapsed;

                if (int.Parse(resp) == 1)
                {
                    var dlg = new MessageDialog("Sharing Completed");
                    await dlg.ShowAsync();
                }
                else if (int.Parse(resp) == 2)
                {
                    var dlg = new MessageDialog("Incomplete!Sharing Destination is offline!!");
                    await dlg.ShowAsync();
                }
                else
                {
                    var dlg = new MessageDialog("Something Wrong with Sharing!");
                    await dlg.ShowAsync();
                }
            }
        }

        private async void AppBarDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ja.Count > 0)
            {
                JsonObject jo = ja[index].GetObject();
                var deleteDialog = new MessageDialog("Are you sure you want to delete this item: " +
                                                 jo.GetNamedString("ItemName") + "?", "Delete Item");
                deleteDialog.Commands.Add(new UICommand("Yes",
                    new UICommandInvokedHandler(this.CommandInvokedHandler), 0));
                deleteDialog.Commands.Add(new UICommand("No", null, 1));
                deleteDialog.DefaultCommandIndex = 1;

                await deleteDialog.ShowAsync();
            }   
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            ja.RemoveAt(index);
            ItemList.Items.Clear();
            AddItemToList();
            InitializePage();
            index = 0;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs eventArgs)
        {
            BigImage.Source = ItemImage.Source;
            BigImage.Visibility = Visibility.Visible;
            BigImage.Tapped += BigImage_Tapped;
        }

        private void BigImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BigImage.Visibility = Visibility.Collapsed;
            BigImage.Tapped -= BigImage_Tapped;
        }
        
    }
}
