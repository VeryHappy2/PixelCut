using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ChangeImage.Service;
using System.Drawing;
using ChangeImage.Services;

namespace ChangeImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageProcessingService _imageProcessingService { get; set; }
        private ManagmentImageService _managmentService { get; set; }
        private BitmapImage _image;

        public MainWindow()
        {
            InitializeComponent();
            _imageProcessingService = new ImageProcessingService();
            _managmentService = new ManagmentImageService();
        }

        private async void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;)|*.png;*.jpg;";

            if (openFileDialog.ShowDialog() == true)
            {
                LoadedImage.Source = null;
                _image = null!;
                ValueSlider.Value = 0;

                if (!Directory.Exists("Cache"))
                {
                    Directory.CreateDirectory("Cache");
                }

                ValueSlider.IsEnabled = false;

                string folderPath = Directory.GetCurrentDirectory();
                string fullPath = Path.Combine(folderPath, "Cache");

                await Task.Run(() => _managmentService.DeleteImagesFromData(fullPath));
                BitmapImage bitmap = _imageProcessingService.LoadImage(openFileDialog.FileName);
                LoadedImage.Source = bitmap;
                _image = bitmap;

                await CalculateImagesAsync(_image, openFileDialog.FileName);
            }
        }

        private async void ValueSlider_ValueChanged(object slender, RoutedEventArgs e)
        {
            await Task.Delay(520);
            sbyte percents = (sbyte)ValueSlider.Value;

            if (percents == 0)
            {
                LoadedImage.Source = _image;
                SliderValueText.Text = $"Value: 0%";
                return;
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(currentDirectory, "Cache", $"image{percents}.png");

            if (!File.Exists(fullPath))
            {
                return;
            }

            LoadedImage.Source = _imageProcessingService.LoadImage(fullPath);
            SliderValueText.Text = $"Value: {ValueSlider.Value}%";
        }

        private void SaveImage_Click(object slender, RoutedEventArgs e)
        {
            if (LoadedImage.Source == null) 
                return;

            BitmapSource bitmapSource = LoadedImage.Source as BitmapSource;

            _managmentService.SaveImageWithDialog(bitmapSource);
        }

        private async Task CalculateImagesAsync(BitmapImage bitmap, string path) 
        {
            string folderPath = "Cache";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            for (int i = 1; i < 100; i++)
            {
                var writeableBitmap = await _imageProcessingService.DivideImagePixelServiceAsync(bitmap, percents: i, path);
                string filePathCreate = Path.Combine(folderPath, $"image{i}.png"); 

                SaveWriteableBitmapToFile(writeableBitmap, filePathCreate);
                ValueSlider.IsEnabled = true;
            }
        }

        private void SaveWriteableBitmapToFile(WriteableBitmap writeableBitmap, string path)
        {
            try
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while the {path} was deleting: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}