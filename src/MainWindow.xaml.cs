using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;


namespace ChangeImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage _image;
        private List<BitmapSource> _images;
        public MainWindow()
        {
            InitializeComponent();
            _images = new List<BitmapSource>();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;)|*.png;*.jpg;";

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.EndInit();
                _image = bitmap;
                LoadedImage.Source = bitmap;
                CulcalateImages(_image, openFileDialog.FileName);
            }
        }

        private async void ValueSlider_ValueChanged(object slender, RoutedEventArgs e)
        {
            await Task.Delay(520);
            sbyte percents = (sbyte)ValueSlider.Value;

            if (_image == null || _images == null)
                return;

            if (percents == 0)
            {
                LoadedImage.Source = _image;
                SliderValueText.Text = $"Value: 0%";
                return;
            }
            
            int index = percents - 1;
            
            LoadedImage.Source = _images[index];
            SliderValueText.Text = $"Value: {ValueSlider.Value}%";
        }

        private void SaveImage_Click(object slender, RoutedEventArgs e)
        {
            if (LoadedImage.Source == null) 
                return;

            BitmapSource bitmapSource = LoadedImage.Source as BitmapSource;

            ImageProcessingService.ImageProcessingService services = new ImageProcessingService.ImageProcessingService();
            services.SaveImageWithDialog(bitmapSource);
        }

        private void CulcalateImages(BitmapImage bitmap, string path) 
        {
            ImageProcessingService.ImageProcessingService services = new ImageProcessingService.ImageProcessingService();

            for (int i = 1; i < 100; i++)
            {
                _images.Add(services.DivideImagePixelServiceAsync(bitmap, percents: i, path));
            }
        }
    }
}