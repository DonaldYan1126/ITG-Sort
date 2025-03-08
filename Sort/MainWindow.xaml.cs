using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Sort
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow(Uri? uri)
        {
            this.InitializeComponent();
            if (uri != null)
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string folderPath = queryParams["folder"];
                if (!string.IsNullOrEmpty(folderPath))
                {
                    LoadFiles(folderPath);
                }
                else
                {
                    FilesListView.Items.Add("Transfer error, please try again.");
                }
            }
            else
            {
                FilesListView.Items.Add("Please click Sort Academic Assets in Right-Click menu to use the App");
            }
        }

        private void LoadFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        FilesListView.Items.Add(Path.GetFileName(file));
                    }
                }
                catch (Exception ex)
                {
                    FilesListView.Items.Add($"Failed to get Files：{ex.Message}");
                }
            }
            else
            {
                FilesListView.Items.Add("Target file doesn't exist！");
            }
        }
    }
}
