using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace appsdk_test
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 


    public sealed partial class MainWindow : Window
    {

        public ObservableCollection<StorageFile> images = new();

        public MainWindow()
        {
            this.InitializeComponent();
            ConvertButton.IsEnabled = false;
            //SetTitleBar(TBar);

        }

        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);


            FileOpenPicker openPicker = new();
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".webp");
            openPicker.FileTypeFilter.Add(".tif");
            openPicker.FileTypeFilter.Add(".tiff");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".jfif");


            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (!images.Where((f) => f.Path == file.Path).Any())
                        images.Add(file);

                }
            }
            else
            {
            }

            ConvertButton.IsEnabled = images.Count > 0 && FormatCB.SelectedItem != null;

        }

        private void FormatCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConvertButton.IsEnabled = images.Count > 0 && FormatCB.SelectedItem != null;
            WebpQualityPicker.Visibility = FormatCB.SelectedValue.ToString() == "WEBP"? Visibility.Visible : Visibility.Collapsed; 
        }

        private void RemoveImageClick(object sender, RoutedEventArgs e)
        {
            if (rcItem is null) return;

            images.Remove(rcItem);
        }

        async void Convert(string output)
        {
            ConvertButton.Label = "Stop";
            ConvertButton.Icon = new FontIcon { Glyph = "\uE71A" };

            ProgressBar.Maximum = images.Count;
            ProgressBar.Value = 0;


            var progress = new Progress<int>(s =>
            {
                ProgressBar.Value = s;
            });

            var _images = new List<StorageFile>(images);
            var _format = FormatCB.SelectedValue.ToString();
            var _output = output;
            WorkerThread.Stop = false;
            int quality = Webp_Quality.Value == 110 ? -1 : (int)Webp_Quality.Value;
            await Task.Factory.StartNew(() => WorkerThread.LongWork(progress, _images, _output, _format, quality),
                                 TaskCreationOptions.LongRunning);


            ConvertButton.Label = "Convert";
            ConvertButton.Icon = new FontIcon { Glyph = "\uE768" };

            ContentDialog dialog = new();
            dialog.Title = "Convertion complete";

            dialog.SecondaryButtonText = "Open Folder";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = $"{ProgressBar.Value} items Converted successfully";
            dialog.SecondaryButtonClick += delegate
            {
                Process.Start("explorer.exe", output);
            };

            dialog.XamlRoot = this.Content.XamlRoot;
            var result = await dialog.ShowAsync();


        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkerThread.Stop)
            {
                WorkerThread.Stop = true;
                return;
            }

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);


            FolderPicker openPicker = new();
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;



            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                Convert(folder.Path);

            }
            else
            {
            }

        }

        private async void PreviewButton_Click(object sender, RoutedEventArgs e)
        {

            if (rcItem is null) return;

            var path = rcItem.Path;
            ContentDialog dialog = new();
            dialog.Title = "Preview";

            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new Microsoft.UI.Xaml.Controls.Image
            {
                Source = new BitmapImage
                {
                    UriSource = new Uri(path),

                },

            };



            dialog.XamlRoot = this.Content.XamlRoot;
            await dialog.ShowAsync();

        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (rcItem is null) return;

            await Windows.System.Launcher.LaunchFileAsync(rcItem);

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            images.Clear();
        }

        private async void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);


            FolderPicker openPicker = new();
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;



            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                foreach (var file in await folder.GetFilesAsync())
                {

                    var types = new string[] { ".jpg", ".jpeg", ".png", ".webp", ".tif", ".tiff", ".gif", ".jfif" };

                    if (types.Contains(file.FileType.ToLowerInvariant()))
                    {
                        images.Add(file);
                    }
                }

            }
            else
            {
            }
            //((IInitializeWithWindow)(object)picker).Initialize(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

            ConvertButton.IsEnabled = images.Count > 0 && FormatCB.SelectedItem != null;
        }


        StorageFile rcItem;

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView listView = (ListView)sender;
            allContactsMenuFlyout.ShowAt(listView, e.GetPosition(listView));
            var a = ((FrameworkElement)e.OriginalSource).DataContext;
            rcItem = a as StorageFile;
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {

            ContentDialog dialog = new();
            dialog.Title = "About";

            dialog.CloseButtonText = "Ok";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Winui Image Converter V1.0.0";

            dialog.XamlRoot = this.Content.XamlRoot;
            await dialog.ShowAsync();
        }
    }

    public class WorkerThread
    {
        public static bool Stop = true;
        readonly static Dictionary<string, ImageFormat> formatMap = new()
        {
            { "JPEG", ImageFormat.Jpeg },
            { "PNG", ImageFormat.Png },
            { "BMP", ImageFormat.Bmp },
            { "GIF", ImageFormat.Gif },
            { "TIFF", ImageFormat.Tiff },
        };
        public static void LongWork(IProgress<int> progress, List<StorageFile> images, string output, string format,int webpQuality=-1)
        {
            int p = 0;

            foreach (var row in images)
            {
                byte[] buffer = File.ReadAllBytes(row.Path);

                Bitmap bitmap = null;

                //check if current image is webp and encode
                if (row.FileType.ToLowerInvariant().Contains("webp"))
                {
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    bitmap = decoder.DecodeFromBytes(buffer, buffer.LongLength);
                }
                else // If not, load file into bitmap
                {
                    bitmap = new Bitmap(new MemoryStream(buffer));

                }

                System.Drawing.Image img = bitmap;

                if (format != "WEBP")
                {

                    img.Save(System.IO.Path.Combine(output, row.Name) + $".{format}", formatMap[format]);
                }
                else
                {
                    //If saving as webp encode the file
                    var encoder = new Imazen.WebP.SimpleEncoder();
                    FileStream outStream = new($"{System.IO.Path.Combine(output, row.Name)}.webp", FileMode.Create);
                    encoder.Encode(bitmap, outStream, webpQuality);
                    outStream.Close();
                }
                p++;
                progress.Report(p);
                if (Stop) return;
            }
            Stop = true;

        }
    }
}
