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
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.Web.Http;

namespace HoloPi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LaunchingPage : Page
    {
        // times of History Connection, only keep up to 5 connections on the list
        private int HC_count = 0; 
        
        // Raspberry Pi IP address
        private string DestIP = "";

        public LaunchingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;          
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            // get the Raspberry IP address
            SetIPAddress();
            
            
            // add http header and the port number
            string DestUri = "http://" + DestIP + ":5000/";

            // authenticate the connection
            Connecting(DestUri);
        }

        // get the ip address and check if it is a valid ip address
        private void SetIPAddress()
        {
            if (string.IsNullOrEmpty(IP1.Text) || string.IsNullOrEmpty(IP2.Text)
                || string.IsNullOrEmpty(IP3.Text) || string.IsNullOrEmpty(IP4.Text))
            {
                return; // use the DestIP created by the history connection(if selected)
            }
            else
            {
                // retreive the text from four text boxes to form a valid Raspberry Pi IP address
                DestIP = IP1.Text + "." + IP2.Text + "." + IP3.Text + "." + IP4.Text;
            }
            
        }

        // verify the connection with entered IP address by sending a simple request to this IP
        private async void Connecting(string uri)
        {
            
            try
            {
                pring.IsActive = true;
                pring.Visibility = Visibility.Visible;
                this.Frame.IsEnabled = false;

                HttpClient httpClient = new HttpClient();
                string response = await httpClient.GetStringAsync(new Uri(uri));
                
                if (int.Parse(response) == 1)
                {
                    httpClient.Dispose(); 
                
                    AfterConnected();
                }
            }
            catch (Exception e)
            {
                TxtError.Visibility = Visibility.Visible;
                DestIP = "";
            }
            finally
            {
                pring.IsActive = false;
                pring.Visibility = Visibility.Collapsed;
                this.Frame.IsEnabled = true;
            }
        }

        private void AfterConnected()
        {
            AddIPToList();

            TxtError.Visibility = Visibility.Collapsed;

            this.Frame.Navigate(typeof(DetectionPage), DestIP);
        }

        // add the valid IP address to History Connection List
        private void AddIPToList()
        {
            if (HC_count >= 5)
            {
                HCList.Items.RemoveAt(1);
            }

            ListViewItem item = new ListViewItem
            {
                Content = DestIP,
                FontSize = 20,
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Center   
            };
            item.Tapped += Item_Tapped;
            
            HCList.Items.Add(item);
            HC_count++;
        }

        // if history connection is selected, retreive the ip and assign it to DestIP
        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            DestIP = item.Content.ToString();
        }

        // when come back to this page, clear all text boxes and reset DestIP
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ClearIpBoxes();
        }

        private void ClearIpBoxes()
        {
            IP1.Text = "";
            IP2.Text = "";
            IP3.Text = "";
            IP4.Text = "";
            DestIP = "";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearIpBoxes();
        }
    }
}
