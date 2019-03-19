using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HoloPi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReceivingPageTwo : Page
    {
        JsonArray ja;

        // selected ListViewItem index
        int index;

        public ReceivingPageTwo()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(100, 100);
            ApplicationView.PreferredLaunchWindowingMode =
                ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().TryResizeView(new Size(100, 100));
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ja = JsonArray.Parse(e.Parameter.ToString());
            AddItemToList();
        }

        private void AddItemToList()
        {
            int itemCounts = ja.Count;
            PageTitle.Text = "From" + ja[0].GetObject().GetNamedString("sourceIp");

            for (int i = 1; i < itemCounts; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Content = "" + i + "." + ja[i].GetObject().GetNamedString("ItemName");
                item.FontSize = 40;
                item.Tapped += Item_Tapped;
                ItemList.Items.Add(item);
            }
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            index = int.Parse(item.Content.ToString().Split('.')[0]);
            var jo = ja[index].GetObject();
            SetImage(jo);
        }

        private async void SetImage(JsonObject jo)
        {
            var bytes = Convert.FromBase64String(jo.GetNamedString("ItemImage"));
            var Imagebuf = bytes.AsBuffer();
            var Imagestream = Imagebuf.AsStream();
            BitmapImage bmpImage = new BitmapImage();
            await bmpImage.SetSourceAsync(Imagestream.AsRandomAccessStream());

            BigImage.Source = bmpImage;
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
