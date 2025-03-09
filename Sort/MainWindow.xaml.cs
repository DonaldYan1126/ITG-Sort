using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Sort
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<FileItem> FileItems { get; } = new();
        public ObservableCollection<Subject> Subjects { get; } = new();

        ProgressDialog progressDialog = new ProgressDialog();
        string folderPath = null;
        private string _documentsFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public MainWindow(Uri? uri)
        {
            WindowManager.Get(this).IsMinimizable = false;
            WindowManager.Get(this).IsMaximizable = false;
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
            MainPage mainPage = new MainPage(uri);
            Content = mainPage;
        }


    }

}
