using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class ReceivingPage : Page
    {
        JsonArray ja;

        // selected ListViewItem index
        int index;

        public ReceivingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ja = JsonArray.Parse(e.Parameter.ToString());
            AddItemToList();

            ItemName.Text = ja[0].GetObject().GetNamedString("ItemName");
            ItemDescription.Text = ja[0].GetObject().GetNamedString("ItemDescription");
            SetImage(ja[0].GetObject());
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

        private void ItemImage_Tapped(object sender, TappedRoutedEventArgs e)
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
