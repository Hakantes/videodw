using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using YouTubeDownloader.Mac.ViewModels;

namespace YouTubeDownloader.Mac.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnBrowseFolderClicked(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "İndirme Klasörünü Seçin",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                var path = folders[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(path) && DataContext is MainWindowViewModel vm)
                {
                    vm.DownloadDirectory = path;
                }
            }
        }
    }
}
