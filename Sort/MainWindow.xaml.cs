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

        private string _documentsFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public MainWindow(Uri? uri)
        {
            InitializeSubjects();
            InitializeComponent();
            FilesListView.ItemsSource = FileItems;
            FilesListView.DataContext = this;
            WindowManager.Get(this).IsMinimizable = false;
            WindowManager.Get(this).IsMaximizable = false;
            ExtendsContentIntoTitleBar = true;
            CheckConfigStatus();
            SetTitleBar(AppTitleBar);
            SystemBackdrop = new MicaBackdrop()
            { Kind = MicaKind.BaseAlt };
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
                FileItems.Add(new FileItem
                {
                    OriginalPath = "",
                    FileName = Path.GetFileName("Please click \"Sort Academic Assets\" in Right-Click menu to use the App"),
                    TargetSubject = DetectSubject(Path.GetFileName(""))
                }); ;
            }
        }

        private void CheckConfigStatus()
        {
            var defaultUsed = !File.Exists(ConfigLoader.ConfigPath);
            ConfigStatus.Text = defaultUsed ?
                "⚠ Using the Default Configuration ":
                "✅ Loaded the external Configuration:" + ConfigLoader.ConfigPath ;
            ConfigStatus.Foreground = defaultUsed ?
                new SolidColorBrush(Colors.Orange) :
                new SolidColorBrush(Colors.Green);
        }


        private void InitializeSubjects()
        {
            Subjects.Clear();
            foreach (var subject in ConfigLoader.LoadSubjects())
            {
                Subjects.Add(subject);
            }
        }


        private void AutoSortButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FileItems.Where(f => f.IsUncategorized))
            {
                item.TargetSubject = DetectSubject(item.FileName);
            }
        }

        private Subject? DetectSubject(string fileName)
        {
            return Subjects.FirstOrDefault(subject =>
                subject.Patterns.Any(pattern =>
                    pattern.IsMatch(Path.GetFileNameWithoutExtension(fileName))));
        }

        private async void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: FileItem item })
            {
                try
                {
                    var targetDir = Path.Combine(_documentsFolder, item.TargetSubject.Name);
                    if(!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                    var targetPath = Path.Combine(targetDir, item.FileName);

                    // 处理重名文件
                    int counter = 1;
                    while (File.Exists(targetPath))
                    {
                        targetPath = Path.Combine(targetDir,
                            $"{Path.GetFileNameWithoutExtension(item.FileName)} ({counter})" +
                            $"{Path.GetExtension(item.FileName)}");
                        counter++;
                    }

                    File.Move(item.OriginalPath, targetPath);
                    item.IsProcessed = true;
                }
                catch (Exception ex)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Failed to Move",
                        Content = ex.Message,
                        CloseButtonText = "Confirm",
                        XamlRoot = Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
        }

        private void LoadFiles(string folderPath)
        {
            FileItems.Clear();
            if (Directory.Exists(folderPath))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(folderPath))
                    {
                        FileItems.Add(new FileItem
                        {
                            OriginalPath = file,
                            FileName = Path.GetFileName(file),
                            TargetSubject = DetectSubject(Path.GetFileName(file))
                        });
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            else
            {
                FileItems.Add(new FileItem
                {
                    OriginalPath = "",
                    FileName = Path.GetFileName("Error!"),
                    TargetSubject = DetectSubject(Path.GetFileName(""))
                });
            }
        }

        async Task Move(List<FileItem> itemsToMove)
        {
            try
            {
                int successCount = 0;
                int errorCount = 0;
                var errorMessages = new List<string>();
                
                await Task.Run(async () =>
                {
                    foreach (var item in itemsToMove)
                    {
                        try
                        {
                            DispatcherQueue.TryEnqueue(() =>
                                progressDialog.SetProgress(item.FileName));

                            MoveFile(item);
                            successCount++;
                            await Task.Delay(200);
                        }
                        catch (Exception ex)
                        {
                            if (string.IsNullOrWhiteSpace(ex.Message))
                                {
                                errorCount++;
                                errorMessages.Add($"【{item.FileName}】{ex.Message}");
                            }
                            else successCount++;
                            await Task.Delay(200);
                        }
                    }
                });

                progressDialog.Hide();

                var resultMessage = $"Mission Complete!\nSucceeded: {successCount} \nFailed{errorCount} ";
                if (errorMessages.Any())
                {
                    resultMessage += "\n\nError details:\n" + string.Join("\n", errorMessages);
                }

                await ShowMessageDialog("Result", resultMessage);
            }
            finally
            {
                progressDialog.Hide();
            }
        }

        private async void MoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsToMove = FileItems
                .Where(f => f.CanMove)
                .ToList();

            if (!itemsToMove.Any())
            {
                await ShowMessageDialog("Attention", "Nothing to move");
                return;
            }

            progressDialog = new ProgressDialog();
            progressDialog.XamlRoot = Content.XamlRoot;
            var result = progressDialog.ShowAsync();
            await Move(itemsToMove);
        }
        private string GetUniquePath(string directory, string fileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;

            var newPath = Path.Combine(directory, fileName);

            while (File.Exists(newPath))
            {
                newPath = Path.Combine(directory, $"{baseName} ({counter++}){extension}");
            }
            return newPath;
        }

        private void MoveFile(FileItem item)
        {
            var targetDir = Path.Combine(_documentsFolder, item.TargetSubject.Name);
            Directory.CreateDirectory(targetDir);

            var targetPath = GetUniquePath(targetDir, item.FileName);
            File.Move(item.OriginalPath, targetPath);
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.WrapWholeWords
                    }
                },
                CloseButtonText = "Confirm",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public class FileItem : INotifyPropertyChanged
    {
        public string OriginalPath { get; set; }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        private Subject _targetSubject;
        public Subject TargetSubject
        {
            get => _targetSubject;
            set { _targetSubject = value; OnPropertyChanged(); }
        }

        private bool _isProcessed;
        public bool IsProcessed
        {
            get => _isProcessed;
            set { _isProcessed = value; OnPropertyChanged(); }
        }

        public bool IsUncategorized => TargetSubject == null && !IsProcessed;
        public bool CanMove => TargetSubject != null && !IsProcessed;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Subject
    {
        public string Name { get; set; }
        public List<Regex> Patterns { get; set; } = new();
    }

    public class SubjectConfig
    {
        public string Name { get; set; }
        public List<string> Patterns { get; set; } = new();
    }

    public static class ConfigLoader
    {
        public static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/subjects.json");

        public static List<Subject> LoadSubjects()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return GetDefaultSubjects();

                var json = File.ReadAllText(ConfigPath);
                var configs = JsonSerializer.Deserialize<List<SubjectConfig>>(json);

                return configs.Select(c => new Subject
                {
                    Name = c.Name,
                    Patterns = c.Patterns
                        .Select(p => new Regex(p, RegexOptions.IgnoreCase))
                        .ToList()
                }).ToList();
            }
            catch
            {
                return GetDefaultSubjects();
            }
        }

        private static List<Subject> GetDefaultSubjects()
        {
            return new List<Subject>
        {
            new Subject
            {
                Name = "Mathematics",
                Patterns = new List<Regex>
                {
                    new Regex(@"math|数学|shuxue", RegexOptions.IgnoreCase),
                    new Regex(@"^\d{4}_calc_.*\.pdf$")
                }
            },
            new Subject
            {
                Name = "Physics",
                Patterns = new List<Regex>
                {
                    new Regex(@"physics|物理|wuli", RegexOptions.IgnoreCase),
                    new Regex(@"^exp_\d+_.*\.docx?$")
                }
            }
        };
        }
    }

}
