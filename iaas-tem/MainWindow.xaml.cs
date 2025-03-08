using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace iaas
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow(Uri? uri)
        {
            // 解析 URI 中的参数
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
                    FilesListView.Items.Add("传输错误，请重试。");
                }
            }
            else
            {
                FilesListView.Items.Add("未检测到文件夹路径，请通过右键菜单启动程序。");
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
                    FilesListView.Items.Add($"读取文件出错：{ex.Message}");
                }
            }
            else
            {
                FilesListView.Items.Add("指定的文件夹不存在！");
            }
        }
    }
}
