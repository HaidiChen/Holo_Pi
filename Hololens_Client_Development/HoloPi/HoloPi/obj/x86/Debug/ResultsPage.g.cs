﻿#pragma checksum "C:\Users\chenh\source\repos\HoloPi\HoloPi\ResultsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "DB51283B27951D3D8A16C5ABBDF80FA5"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HoloPi
{
    partial class ResultsPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // ResultsPage.xaml line 21
                {
                    this.pring = (global::Windows.UI.Xaml.Controls.ProgressRing)(target);
                }
                break;
            case 3: // ResultsPage.xaml line 24
                {
                    global::Windows.UI.Xaml.Controls.CommandBar element3 = (global::Windows.UI.Xaml.Controls.CommandBar)(target);
                    ((global::Windows.UI.Xaml.Controls.CommandBar)element3).Opening += this.CommandBar_Opening;
                    ((global::Windows.UI.Xaml.Controls.CommandBar)element3).Closing += this.CommandBar_Closing;
                }
                break;
            case 4: // ResultsPage.xaml line 71
                {
                    this.BigImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 5: // ResultsPage.xaml line 65
                {
                    this.ItemDescription = (global::Windows.UI.Xaml.Documents.Run)(target);
                }
                break;
            case 6: // ResultsPage.xaml line 53
                {
                    this.ItemImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                    ((global::Windows.UI.Xaml.Controls.Image)this.ItemImage).Tapped += this.Image_Tapped;
                }
                break;
            case 7: // ResultsPage.xaml line 57
                {
                    this.ItemName = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 8: // ResultsPage.xaml line 44
                {
                    this.ItemList = (global::Windows.UI.Xaml.Controls.ListView)(target);
                }
                break;
            case 9: // ResultsPage.xaml line 30
                {
                    global::Windows.UI.Xaml.Controls.AppBarButton element9 = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)element9).Click += this.AppBarShareButton_Click;
                }
                break;
            case 10: // ResultsPage.xaml line 31
                {
                    global::Windows.UI.Xaml.Controls.AppBarButton element10 = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)element10).Click += this.AppBarDeleteButton_Click;
                }
                break;
            case 11: // ResultsPage.xaml line 34
                {
                    global::Windows.UI.Xaml.Controls.AppBarButton element11 = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)element11).Click += this.AppBarBackButton_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

