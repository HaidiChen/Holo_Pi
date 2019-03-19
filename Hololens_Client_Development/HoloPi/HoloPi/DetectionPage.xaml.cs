using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace HoloPi
{
    /// <summary>
    /// Next Page (i.e. ResultsPage) has a delete function with two options:
    /// 
    /// one is fake delete function which only delete the content on that page;
    /// the other is true delete function which permanently delete content from this app.
    /// 
    /// So in this page, prepared two sets of statements for above two options.
    /// if using one set, please comment the other.
    /// </summary>
    public sealed partial class DetectionPage : Page
    {
        // captured photo
        private StorageFile photo;
        
        private string RP_Uri = "";
        private static string RP_IP = "";

        private int stopReceiving = 0;

        // for the true delete function
        //private Dictionary<string, JsonArray> responseDict;

        // for the fake delete function
        private Dictionary<string, string> responseDict;

        public static string GetIP()
        {
            return RP_IP;
        }

        public DetectionPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            // for the true delete function
            //responseDict = new Dictionary<string, JsonArray>();

            // for the fake delete function
            responseDict = new Dictionary<string, string>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                // get the Raspberry Pi IP address from previous page
                RP_IP = e.Parameter.ToString();
                RP_Uri = "http://" + RP_IP + ":5000/detect";
                title.Text = $"Connected to " + RP_IP;
            }
            debug.Visibility = Visibility.Collapsed;
            receiveBtn.PointerEntered += ReceiveBtn_PointerEntered;

            /*
            Task.Run(async () =>
                {
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = new HttpResponseMessage();
                    string receivedString;
                    while (stopReceiving == 0)
                    {
                        response = await httpClient.GetAsync(new Uri("http://" + RP_IP + ":5000/notify"));
                        receivedString = await response.Content.ReadAsStringAsync();

                        if (int.Parse(receivedString) == 1)
                        {
                            receiveBtn.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
                        }
                    }
                    httpClient.Dispose();
                }
            );
            */

            //if using true delete function, uncomment following statements
            /* 
            foreach (ListViewItem item in DetectionList.Items)
            {
                string key = item.Content.ToString();
                if (responseDict.TryGetValue(key, out JsonArray value))
                {
                    if (value.Count == 0)
                    {
                        DetectionList.Items.Remove(item);
                        responseDict.Remove(key);
                    }
                }
            }
            */
        }

        private async void ReceiveBtn_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            
            response = await httpClient.GetAsync(new Uri("http://" + RP_IP + ":5000/notify"));
            string receivedString = await response.Content.ReadAsStringAsync();

            if (int.Parse(receivedString) == 1)
            {
                receiveBtn.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
            }
            
            httpClient.Dispose();
        }

        private void DetectBtn_Click(object sender, RoutedEventArgs e)
        {
            CaptureImage();            
        }

        // take a photo
        private async void CaptureImage()
        {
            /*
             * capture the image using camera
             */
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

            photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }
            string filename = "" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + 
                                    "_" + DateTime.Now.Second + ".jpg";
            await photo.RenameAsync(filename, NameCollisionOption.GenerateUniqueName);

            UploadImage();
        }

        private async void UploadImage()
        {
            // prepare the request content
            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            HttpStreamContent streamfile = new HttpStreamContent(stream);
            HttpMultipartFormDataContent httpContents = new HttpMultipartFormDataContent();
            httpContents.Add(streamfile, "file", photo.Name);
            
            //send request
            SendRequest(httpContents);
        }

        private async void SendRequest(HttpMultipartFormDataContent httpContents)
        {
            var client = new HttpClient();
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                this.Frame.IsEnabled = false;
                pring.IsActive = true;
                pring.Visibility = Visibility.Visible;

                result = await client.PostAsync(new Uri(RP_Uri), httpContents);
                string response = await result.Content.ReadAsStringAsync();

                // notice that 'split()' method is used to drop the extension ".jpg"
                string key = photo.Name.Split('.')[0];
                AddRespToDictionary(key, response);
                AddDetectionToList(key);

                client.Dispose();
                await photo.DeleteAsync();

                // for the fake delete function
                this.Frame.Navigate(typeof(ResultsPage), JsonArray.Parse(response));
                
                /* 
                 * below is for the true delete function
                 * 
                if (responseDict.TryGetValue(key, out JsonArray foundValue))
                {
                    this.Frame.Navigate(typeof(ResultsPage), foundValue);
                }
                */

            }
            catch (Exception e)
            {
                debug.Visibility = Visibility.Visible;
            }
            finally
            {
                this.Frame.IsEnabled = true;
                pring.IsActive = false;
                pring.Visibility = Visibility.Collapsed;
            }
        }

        // convert the response to JsonArray and put it into the 
        // Dictionary as a value while using filename as the key
        private void AddRespToDictionary(string key, string response)
        {
            // for the true delete function
            /*
            JsonArray responseJA = JsonArray.Parse(response);
            responseDict.Add(key, responseJA);
            */

            // for the fake delete function
            responseDict.Add(key, response);
        }

        // use unique filename as detectionID and put it into
        // the Detection List
        private void AddDetectionToList(string detectionID)
        {
            ListViewItem item = new ListViewItem
            {
                Content = detectionID,
                FontSize = 30,
            };
            item.Tapped += Item_Tapped;

            DetectionList.Items.Add(item);
        }

        // when a particular detection record is selected
        // navigate to the ResultsPage along with corresponding
        // JsonArray response received before
        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;

            // for the true delete function
            /*
            if (responseDict.TryGetValue(item.Content.ToString(), out JsonArray foundValue))
            {
                this.Frame.Navigate(typeof(ResultsPage), foundValue);
            }
            */

            // for the fake delete function
            string js = responseDict.GetValueOrDefault(item.Content.ToString());
            this.Frame.Navigate(typeof(ResultsPage), JsonArray.Parse(js));
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            // clear all the resources generated by this connection
            ClearAllResources();
            //stopReceiving = 1;
            this.Frame.Navigate(typeof(LaunchingPage));
        }

        private void ClearAllResources()
        {
            // clear the detection list after disconnecting
            DetectionList.Items.Clear();
            
            // also clear the responseDict Dictionary
            responseDict.Clear();
        }
        
        private async void ReceiveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.IsEnabled = false;
            pring.IsActive = true;
            pring.Visibility = Visibility.Visible;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();
            response = await httpClient.GetAsync(new Uri("http://" + RP_IP + ":5000/receive"));
            string receivedString = await response.Content.ReadAsStringAsync();
            
            try
            {
                int.Parse(receivedString);
                var dlg = new MessageDialog("Nothing To Receive");
                await dlg.ShowAsync();
            }
            catch (FormatException fe)
            {
                receiveBtn.Background = new SolidColorBrush(Windows.UI.Colors.Black);

                CoreApplicationView newCoreView = CoreApplication.CreateNewView();

                ApplicationView newAppView = null;
                int mainViewId = ApplicationView.GetApplicationViewIdForWindow(
                                                    CoreApplication.MainView.CoreWindow);

                await newCoreView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    newAppView = ApplicationView.GetForCurrentView();
                    Window.Current.Content = new Frame();
                    (Window.Current.Content as Frame).Navigate(typeof(ReceivingPageTwo), receivedString);
                    Window.Current.Activate();
                });
                      
            

                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                  newAppView.Id,
                  ViewSizePreference.UseHalf,
                  mainViewId,
                  ViewSizePreference.UseHalf
                  );
            }
            finally
            {
                this.Frame.IsEnabled = true;
                pring.IsActive = false;
                pring.Visibility = Visibility.Collapsed;
            }
            
            httpClient.Dispose();
        }
    }
}
